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

namespace MassTransit.Extensions.Hosting.RabbitMq
{
    /// <summary>
    /// Provides extension methods for <see cref="IMassTransitBuilder"/> to configure RabbitMQ bus instances.
    /// </summary>
    public static class UseRabbitMqExtensions
    {
        /// <summary>
        /// Configures a RabbitMq bus.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostAddress">The URI host address of the RabbitMQ host (example: rabbitmq://host:port/vhost).</param>
        /// <param name="hostConfigurator">The configuration callback to configure the RabbitMq bus.</param>
        public static void UseRabbitMq(this IMassTransitBuilder builder, string connectionName, Uri hostAddress,
            Action<IRabbitMqHostBuilder> hostConfigurator)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (hostAddress == null)
                throw new ArgumentNullException(nameof(hostAddress));

            var hostBuilder = new RabbitMqHostBuilder(builder.Services, connectionName, hostAddress);
            hostConfigurator?.Invoke(hostBuilder);
        }

        /// <summary>
        /// Configures a RabbitMq bus.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="host">The host name of the RabbitMq broker.</param>
        /// <param name="virtualHost">The virtual host to use.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the RabbitMq bus.</param>
        public static void UseRabbitMq(this IMassTransitBuilder builder, string connectionName, string host,
            string virtualHost, Action<IRabbitMqHostBuilder> hostConfigurator)
        {
            UseRabbitMq(builder, connectionName, host, RabbitMqOptions.DefaultPort, virtualHost, hostConfigurator);
        }

        /// <summary>
        /// Configures a RabbitMq bus by using the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>.
        /// <param name="options"><see cref="RabbitMqOptions"/></param>
        /// <param name="hostConfigurator">The configuration callback to configure the RabbitMq bus.</param>
        public static void UseRabbitMq(this IMassTransitBuilder builder, RabbitMqOptions options,
            Action<IRabbitMqHostBuilder> hostConfigurator)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            UseRabbitMq(builder, options.ConnectionName, options.Host, options.Port, options.VirtualHost, hostBuilder =>
            {
                hostBuilder.UseUsername(options.Username);
                hostBuilder.UsePassword(options.Password);

                if (options.Heartbeat.HasValue)
                    hostBuilder.UseHeartbeat(options.Heartbeat.Value);

                hostConfigurator?.Invoke(hostBuilder);
            });
        }

        /// <summary>
        /// Configures a RabbitMq bus by using the specified settings.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>.
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="host">The host name of the RabbitMq broker.</param>
        /// <param name="port">The port to connect to on the RabbitMq broker.</param>
        /// <param name="virtualHost">The virtual host to use.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the RabbitMq bus.</param>
        public static void UseRabbitMq(this IMassTransitBuilder builder, string connectionName, string host,
            ushort port, string virtualHost, Action<IRabbitMqHostBuilder> hostConfigurator)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (host == null)
                throw new ArgumentNullException(nameof(host));
            if (virtualHost == null)
                throw new ArgumentNullException(nameof(virtualHost));

            var hostBuilder = new RabbitMqHostBuilder(builder.Services, connectionName, host, port, virtualHost);
            hostConfigurator?.Invoke(hostBuilder);
        }
    }
}