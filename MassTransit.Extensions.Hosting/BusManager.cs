using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace MassTransit.Extensions.Hosting
{
    /// <summary>
    /// Provides an abstraction to manage and return Bus instances.
    /// </summary>
    public interface IBusManager : IHostedService
    {
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
    public class BusManager : IBusManager
    {
        private readonly ConcurrentDictionary<string, Lazy<IBusControl>> _cache = new ConcurrentDictionary<string, Lazy<IBusControl>>();

        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IBusHostFactory> _busHostFactoryList;

        /// <summary>
        /// Initializes a new instance of the <see cref="BusManager"/> class.
        /// </summary>
        public BusManager(IServiceProvider serviceProvider, IEnumerable<IBusHostFactory> busHostFactoryList)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _busHostFactoryList = busHostFactoryList ?? throw new ArgumentNullException(nameof(busHostFactoryList));
        }

        /// <inheritdoc />
        public virtual IBus GetBus(string connectionName)
        {
            connectionName = connectionName ?? string.Empty;

            var factory = _busHostFactoryList.FirstOrDefault(_ => _.ConnectionName == connectionName);
            if (factory == null)
                throw new KeyNotFoundException();

            return GetBus(factory);
        }

        private IBusControl GetBus(IBusHostFactory factory)
        {
            return _cache.GetOrAdd(factory.ConnectionName, new Lazy<IBusControl>(() => factory.Create(_serviceProvider))).Value;
        }

        /// <inheritdoc />
        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            await ExecuteAsync(bus => bus.StartAsync(cancellationToken)).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            await ExecuteAsync(bus => bus.StopAsync(cancellationToken)).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(Func<IBusControl, Task> action)
        {
            var tasks = _busHostFactoryList.Select(factory => action(GetBus(factory)));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

    }
}