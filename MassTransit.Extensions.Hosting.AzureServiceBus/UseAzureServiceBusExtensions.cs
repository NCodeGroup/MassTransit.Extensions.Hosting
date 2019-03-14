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
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace MassTransit.Extensions.Hosting.AzureServiceBus
{
    /// <summary>
    /// Provides extension methods for <see cref="IMassTransitBuilder"/> to configure AzureServiceBus instances.
    /// </summary>
    public static class UseAzureServiceBusExtensions
    {
        /// <summary>
        /// Configures an AzureServiceBus host using a MassTransit host address.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostAddress">The address of the service bus namespace and accompanying service scope in MassTransit format (sb://namespace.servicebus.windows.net/scope).</param>
        /// <param name="hostConfigurator">The configuration callback to configure the AzureServiceBus.</param>
        public static void UseAzureServiceBus(this IMassTransitBuilder builder, string connectionName, Uri hostAddress, Action<IServiceBusHostBuilder> hostConfigurator = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (hostAddress == null)
                throw new ArgumentNullException(nameof(hostAddress));

            var hostBuilder = new ServiceBusHostBuilder(builder.Services, connectionName, hostAddress);
            hostConfigurator?.Invoke(hostBuilder);
        }

        /// <summary>
        /// Configures an AzureServiceBus host using a connection string (Endpoint=...., etc).
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="connectionString">The connection string in the proper format</param>
        /// <param name="hostConfigurator">The configuration callback to configure the AzureServiceBus.</param>
        public static void UseAzureServiceBus(this IMassTransitBuilder builder, string connectionName, string connectionString, Action<IServiceBusHostBuilder> hostConfigurator = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString));

            // in case they pass a URI by mistake (it happens)
            if (Uri.TryCreate(connectionString, UriKind.Absolute, out var hostAddress))
            {
                UseAzureServiceBus(builder, connectionName, hostAddress, hostConfigurator);
                return;
            }

            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
            var hostBuilder = new ServiceBusHostBuilder(builder.Services, connectionName, namespaceManager.Address);

            hostBuilder.UseTokenProvider(namespaceManager.Settings.TokenProvider);
            hostBuilder.UseOperationTimeout(namespaceManager.Settings.OperationTimeout);

            hostConfigurator?.Invoke(hostBuilder);
        }

        /// <summary>
        /// Configures an AzureServiceBus host by using the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>.
        /// <param name="options"><see cref="ServiceBusOptions"/></param>
        /// <param name="hostConfigurator">The configuration callback to configure the AzureServiceBus.</param>
        public static void UseAzureServiceBus(this IMassTransitBuilder builder, ServiceBusOptions options, Action<IServiceBusHostBuilder> hostConfigurator = null)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            UseAzureServiceBus(builder, options.ConnectionName, options.HostAddress, hostBuilder =>
            {
                hostBuilder.UseTokenProvider(options.TokenProvider);

                if (options.OperationTimeout.HasValue)
                    hostBuilder.UseOperationTimeout(options.OperationTimeout.Value);

                if (options.RetryMinBackoff.HasValue)
                    hostBuilder.UseRetryMinBackoff(options.RetryMinBackoff.Value);

                if (options.RetryMaxBackoff.HasValue)
                    hostBuilder.UseRetryMaxBackoff(options.RetryMaxBackoff.Value);

                if (options.RetryLimit.HasValue)
                    hostBuilder.UseRetryLimit(options.RetryLimit.Value);

                switch (options.TransportType)
                {
                    case TransportType.Amqp:
                        hostBuilder.UseAmqp(options.AmqpTransportSettings);
                        break;

                    case TransportType.NetMessaging:
                        hostBuilder.UseNetMessaging(options.NetMessagingTransportSettings);
                        break;
                }

                if (options.BatchFlushInterval.HasValue)
                    hostBuilder.UseBatchFlushInterval(options.BatchFlushInterval.Value);

                hostConfigurator?.Invoke(hostBuilder);
            });
        }

        /// <summary>
        /// Configures an AzureServiceBus host by using the specified application configuration.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>.
        /// <param name="configuration">The <see cref="IConfiguration"/> being bound.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the AzureServiceBus.</param>
        public static void UseAzureServiceBus(this IMassTransitBuilder builder, IConfiguration configuration, Action<IServiceBusHostBuilder> hostConfigurator = null)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var connectionName = configuration["ConnectionName"];
            var hostBuilder = new ServiceBusHostBuilder(builder.Services, connectionName, configuration);
            hostConfigurator?.Invoke(hostBuilder);
        }

    }
}