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
using System.Collections.Generic;
using MassTransit.AzureServiceBusTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus.Messaging.Amqp;

namespace MassTransit.Extensions.Hosting.AzureServiceBus
{
    /// <summary>
    /// Provides an abstraction to configure and initialize AzureServiceBus instances.
    /// </summary>
    public interface IServiceBusHostBuilder : IBusHostBuilder<IServiceBusHost, IServiceBusBusFactoryConfigurator>
    {
        /// <summary>
        /// Sets the token provider to access the namespace.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns><see cref="IServiceBusHostBuilder"/></returns>
        IServiceBusHostBuilder UseTokenProvider(TokenProvider value);

        /// <summary>
        /// Sets the operation timeout for the messaging factory.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns><see cref="IServiceBusHostBuilder"/></returns>
        IServiceBusHostBuilder UseOperationTimeout(TimeSpan value);

        /// <summary>
        /// Sets the minimum back off interval for the exponential retry policy.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns><see cref="IServiceBusHostBuilder"/></returns>
        IServiceBusHostBuilder UseRetryMinBackoff(TimeSpan value);

        /// <summary>
        /// Sets the maximum back off interval for the exponential retry policy.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns><see cref="IServiceBusHostBuilder"/></returns>
        IServiceBusHostBuilder UseRetryMaxBackoff(TimeSpan value);

        /// <summary>
        /// Sets the retry limit for service bus operations.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns><see cref="IServiceBusHostBuilder"/></returns>
        IServiceBusHostBuilder UseRetryLimit(int value);

        /// <summary>
        /// Sets the messaging protocol to use AMQP with the specified transport settings.
        /// </summary>
        /// <param name="settings"><see cref="AmqpTransportSettings"/></param>
        /// <returns><see cref="IServiceBusHostBuilder"/></returns>
        IServiceBusHostBuilder UseAmqp(AmqpTransportSettings settings);

        /// <summary>
        /// Sets the type of messaging protocol to use.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns><see cref="IServiceBusHostBuilder"/></returns>
        IServiceBusHostBuilder UseTransport(TransportType value);

        /// <summary>
        /// Sets the messaging protocol to use net messaging with the specified transport settings.
        /// </summary>
        /// <param name="settings"><see cref="AmqpTransportSettings"/></param>
        /// <returns><see cref="IServiceBusHostBuilder"/></returns>
        IServiceBusHostBuilder UseNetMessaging(NetMessagingTransportSettings settings);

        /// <summary>
        /// Sets the the batch flush interval to use with the messaging factory.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns><see cref="IServiceBusHostBuilder"/></returns>
        IServiceBusHostBuilder UseBatchFlushInterval(TimeSpan value);
    }

    /// <summary>
    /// Provides an implementation of <see cref="IServiceBusHostBuilder"/>.
    /// </summary>
    public class ServiceBusHostBuilder :
        BusHostBuilder<IServiceBusHost, IServiceBusBusFactoryConfigurator>,
        IServiceBusHostBuilder
    {
        private readonly IList<Action<ServiceBusHostConfigurator>> _hostConfiguratorActions = new List<Action<ServiceBusHostConfigurator>>();
        private readonly Func<IServiceProvider, ServiceBusHostConfigurator> _hostConfiguratorFactory;

        private ServiceBusHostBuilder(IServiceCollection services, string connectionName)
            : base(services, connectionName)
        {
            services.TryAddTransient<IBusFactory<IServiceBusBusFactoryConfigurator>, ServiceBusBusFactory>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostAddress">The address of the service bus namespace and accompanying service scope in MassTransit format (sb://namespace.servicebus.windows.net/scope).</param>
        public ServiceBusHostBuilder(IServiceCollection services, string connectionName, Uri hostAddress)
            : this(services, connectionName)
        {
            if (hostAddress == null)
                throw new ArgumentNullException(nameof(hostAddress));

            _hostConfiguratorFactory = serviceProvider => new ServiceBusHostConfigurator(hostAddress);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> being bound.</param>
        public ServiceBusHostBuilder(IServiceCollection services, string connectionName, IConfiguration configuration)
            : this(services, connectionName)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            services.AddOptions();
            services.Configure<ServiceBusOptions>(ConnectionName, configuration);

            _hostConfiguratorFactory = serviceProvider =>
            {
                var optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<ServiceBusOptions>>();
                var options = optionsSnapshot.Get(ConnectionName);

                var hostConfigurator = new ServiceBusHostConfigurator(options.HostAddress)
                {
                    TokenProvider = options.TokenProvider
                };

                if (options.OperationTimeout.HasValue)
                    hostConfigurator.OperationTimeout = options.OperationTimeout.Value;

                if (options.RetryMinBackoff.HasValue)
                    hostConfigurator.RetryMinBackoff = options.RetryMinBackoff.Value;

                if (options.RetryMaxBackoff.HasValue)
                    hostConfigurator.RetryMaxBackoff = options.RetryMaxBackoff.Value;

                if (options.RetryLimit.HasValue)
                    hostConfigurator.RetryLimit = options.RetryLimit.Value;

                switch (options.TransportType)
                {
                    case TransportType.Amqp:
                        hostConfigurator.TransportType = TransportType.Amqp;
                        hostConfigurator.AmqpTransportSettings = options.AmqpTransportSettings;
                        break;

                    case TransportType.NetMessaging:
                        hostConfigurator.TransportType = TransportType.NetMessaging;
                        hostConfigurator.NetMessagingTransportSettings = options.NetMessagingTransportSettings;
                        break;
                }

                if (options.BatchFlushInterval.HasValue)
                    hostConfigurator.BatchFlushInterval = options.BatchFlushInterval.Value;

                hostConfigurator.AmqpTransportSettings.BatchFlushInterval = hostConfigurator.BatchFlushInterval;
                hostConfigurator.NetMessagingTransportSettings.BatchFlushInterval = hostConfigurator.BatchFlushInterval;

                return hostConfigurator;
            };
        }

        /// <inheritdoc />
        public virtual IServiceBusHostBuilder UseTokenProvider(TokenProvider value)
        {
            _hostConfiguratorActions.Add(configure => configure.TokenProvider = value);
            return this;
        }

        /// <inheritdoc />
        public virtual IServiceBusHostBuilder UseOperationTimeout(TimeSpan value)
        {
            _hostConfiguratorActions.Add(configure => configure.OperationTimeout = value);
            return this;
        }

        /// <inheritdoc />
        public virtual IServiceBusHostBuilder UseRetryMinBackoff(TimeSpan value)
        {
            _hostConfiguratorActions.Add(configure => configure.RetryMinBackoff = value);
            return this;
        }

        /// <inheritdoc />
        public virtual IServiceBusHostBuilder UseRetryMaxBackoff(TimeSpan value)
        {
            _hostConfiguratorActions.Add(configure => configure.RetryMaxBackoff = value);
            return this;
        }

        /// <inheritdoc />
        public virtual IServiceBusHostBuilder UseRetryLimit(int value)
        {
            _hostConfiguratorActions.Add(configure => configure.RetryLimit = value);
            return this;
        }

        /// <inheritdoc />
        public virtual IServiceBusHostBuilder UseTransport(TransportType value)
        {
            _hostConfiguratorActions.Add(configure => configure.TransportType = value);
            return this;
        }

        /// <inheritdoc />
        public virtual IServiceBusHostBuilder UseAmqp(AmqpTransportSettings settings)
        {
            _hostConfiguratorActions.Add(configure =>
            {
                configure.TransportType = TransportType.Amqp;
                configure.AmqpTransportSettings = settings;
                configure.NetMessagingTransportSettings = null;
            });
            return this;
        }

        /// <inheritdoc />
        public virtual IServiceBusHostBuilder UseNetMessaging(NetMessagingTransportSettings settings)
        {
            _hostConfiguratorActions.Add(configure =>
            {
                configure.TransportType = TransportType.NetMessaging;
                configure.NetMessagingTransportSettings = settings;
                configure.AmqpTransportSettings = null;
            });
            return this;
        }

        /// <inheritdoc />
        public virtual IServiceBusHostBuilder UseBatchFlushInterval(TimeSpan value)
        {
            _hostConfiguratorActions.Add(configure =>
            {
                configure.AmqpTransportSettings.BatchFlushInterval = value;
                configure.NetMessagingTransportSettings.BatchFlushInterval = value;
            });
            return this;
        }

        /// <inheritdoc />
        public override IBusControl Create(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger<ServiceBusHostBuilder>();
            var loggerIsEnabled = logger?.IsEnabled(LogLevel.Debug) ?? false;
            if (loggerIsEnabled)
                logger.LogDebug("Creating AzureServiceBus '{0}'", ConnectionName);

            var busFactory = serviceProvider.GetRequiredService<IBusFactory<IServiceBusBusFactoryConfigurator>>();
            var busControl = busFactory.Create(busFactoryConfigurator =>
            {
                var hostConfigurator = _hostConfiguratorFactory(serviceProvider);
                foreach (var hostConfiguratorAction in _hostConfiguratorActions)
                {
                    hostConfiguratorAction(hostConfigurator);
                }

                var host = busFactoryConfigurator.Host(hostConfigurator.Settings);

                Configure(host, busFactoryConfigurator, serviceProvider);
            });

            if (loggerIsEnabled)
                logger.LogDebug("Created AzureServiceBus '{0}'", ConnectionName);

            return busControl;
        }

    }
}