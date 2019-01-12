#region Copyright Preamble

// 
//    Copyright @ 2019 NCode Group
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MassTransit.Extensions.Hosting
{
    /// <summary>
    /// Provides an abstraction to manage and return Bus instances.
    /// </summary>
    public interface IBusManager : IHostedService
    {
        /// <summary>
        /// Triggered when the bus manager has fully started and is about to wait
        /// for a graceful shutdown.
        /// </summary>
        CancellationToken Started { get; }

        /// <summary>
        /// Triggered when the bus manager is performing a graceful shutdown.
        /// Requests may still be in flight. Shutdown will block until this event completes.
        /// </summary>
        CancellationToken Stopping { get; }

        /// <summary>
        /// Triggered when the bus manager is performing a graceful shutdown.
        /// All requests should be complete at this point. Shutdown will block
        /// until this event completes.
        /// </summary>
        CancellationToken Stopped { get; }

        /// <summary>
        /// Returns the <see cref="IBus"/> instance for the specified client-provided
        /// connection name.
        /// </summary>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <returns><see cref="IBus"/></returns>
        IBus GetBus(string connectionName);
    }

    /// <summary>
    /// Provides an implementation of <see cref="IBusManager"/> that creates
    /// Bus instances using <see cref="IBusHostFactory"/>.
    /// </summary>
    public class BusManager : IBusManager, IDisposable
    {
        private readonly IReadOnlyCollection<IBusHostFactory> _busHostFactoryList;

        private readonly ConcurrentDictionary<string, Lazy<IBusControl>> _cache = new ConcurrentDictionary<string, Lazy<IBusControl>>();

        private readonly object _lock = new object();
        private readonly ILogger<BusManager> _logger;

        private readonly IServiceProvider _serviceProvider;

        private readonly CancellationTokenSource _startedSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _stoppedSource = new CancellationTokenSource();
        private readonly CancellationTokenSource _stoppingSource = new CancellationTokenSource();

        private int _state;

        private enum State
        {
            Initial,
            Starting,
            Started,
            Stopping,
            Stopped,
            Faulted,
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusManager"/> class.
        /// </summary>
        public BusManager(IServiceProvider serviceProvider, IEnumerable<IBusHostFactory> busHostFactoryList, ILogger<BusManager> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (busHostFactoryList == null)
                throw new ArgumentNullException(nameof(busHostFactoryList));

            _busHostFactoryList = busHostFactoryList as IReadOnlyCollection<IBusHostFactory> ?? busHostFactoryList.ToArray();
        }

        /// <inheritdoc />
        public CancellationToken Started => _startedSource.Token;

        /// <inheritdoc />
        public CancellationToken Stopping => _stoppingSource.Token;

        /// <inheritdoc />
        public CancellationToken Stopped => _stoppedSource.Token;

        /// <inheritdoc />
        public virtual IBus GetBus(string connectionName)
        {
            lock (_lock)
            {
                var state = (State)Interlocked.CompareExchange(ref _state, (int)State.Started, (int)State.Started);
                if (state != State.Started)
                    throw new InvalidOperationException("The bus manager must be started");

                connectionName = connectionName ?? string.Empty;

                var factory = _busHostFactoryList.FirstOrDefault(_ => _.ConnectionName == connectionName);
                if (factory == null)
                    throw new KeyNotFoundException("A bus with the specified connection name cannot be found");

                return GetBus(factory);
            }
        }

        /// <inheritdoc />
        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            var beforeState =
                (State)Interlocked.CompareExchange(ref _state, (int)State.Starting, (int)State.Initial);
            if (beforeState != State.Initial)
                throw new InvalidOperationException("The bus managed can only be started once");

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Bus manager starting");

            try
            {
                await ExecuteAsync(bus => bus.StartAsync(cancellationToken)).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Interlocked.Exchange(ref _state, (int)State.Faulted);

                _logger.LogCritical(exception, "Bus manager faulted during startup");

                throw;
            }

            Interlocked.Exchange(ref _state, (int)State.Started);

            Notify(_startedSource, "An error occurred starting the bus manager");

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Bus manager started");
        }

        /// <inheritdoc />
        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                var beforeState =
                    (State)Interlocked.CompareExchange(ref _state, (int)State.Stopping, (int)State.Started);
                if (beforeState != State.Started)
                    throw new InvalidOperationException("The bus manager can only be stopped once after it has started");
            }

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Bus manager stopping");

            Notify(_stoppingSource, "An error occurred stopping the bus manager");

            try
            {
                await ExecuteAsync(bus => bus.StopAsync(cancellationToken)).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Interlocked.Exchange(ref _state, (int)State.Faulted);

                _logger.LogCritical(exception, "Bus manager faulted during shutdown");

                throw;
            }

            Interlocked.Exchange(ref _state, (int)State.Stopped);

            Notify(_stoppedSource, "An error occurred stopping the bus manager");

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug("Bus manager stopped");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary />
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            _startedSource.Dispose();
            _stoppingSource.Dispose();
            _stoppedSource.Dispose();
        }

        private IBusControl GetBus(IBusHostFactory factory)
        {
            return _cache.GetOrAdd(factory.ConnectionName, new Lazy<IBusControl>(() => factory.Create(_serviceProvider))).Value;
        }

        private async Task ExecuteAsync(Func<IBusControl, Task> action)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            var tasks = _busHostFactoryList.Select(factory => action(GetBus(factory)));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        private void Notify(CancellationTokenSource source, string errorMessage)
        {
            try
            {
                // Noop if already cancelled
                if (source.IsCancellationRequested)
                {
                    return;
                }

                // Execute cancellation token callbacks
                source.Cancel(throwOnFirstException: false);
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, errorMessage);
            }
        }

    }
}