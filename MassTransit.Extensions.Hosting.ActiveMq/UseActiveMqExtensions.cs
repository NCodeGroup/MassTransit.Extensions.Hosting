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

namespace MassTransit.Extensions.Hosting.ActiveMq
{
    /// <summary>
    /// Provides extension methods for <see cref="IMassTransitBuilder"/> to configure ActiveMQ bus instances.
    /// </summary>
    public static class UseActiveMqExtensions
    {
        /// <summary>
        /// Configures a ActiveMQ bus.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostAddress">The URI host address of the ActiveMQ host (example: activemq://host:port/).</param>
        /// <param name="hostConfigurator">The configuration callback to configure the ActiveMQ bus.</param>
        public static void UseActiveMq(this IMassTransitBuilder builder, string connectionName, Uri hostAddress, Action<IActiveMqHostBuilder> hostConfigurator = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (hostAddress == null)
                throw new ArgumentNullException(nameof(hostAddress));

            var hostBuilder = new ActiveMqHostBuilder(builder.Services, connectionName, hostAddress);
            hostConfigurator?.Invoke(hostBuilder);
        }

        /// <summary>
        /// Configures a ActiveMQ bus.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="host">The host name of the ActiveMQ broker.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the ActiveMQ bus.</param>
        public static void UseActiveMq(this IMassTransitBuilder builder, string connectionName, string host, Action<IActiveMqHostBuilder> hostConfigurator = null)
        {
            UseActiveMq(builder, connectionName, host, null, hostConfigurator);
        }

        /// <summary>
        /// Configures a ActiveMQ bus by using the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>.
        /// <param name="options"><see cref="ActiveMqOptions"/></param>
        /// <param name="hostConfigurator">The configuration callback to configure the ActiveMQ bus.</param>
        public static void UseActiveMq(this IMassTransitBuilder builder, ActiveMqOptions options, Action<IActiveMqHostBuilder> hostConfigurator = null)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            UseActiveMq(builder, options.ConnectionName, options.Host, options.Port, hostBuilder =>
            {
                if (options.UseSsl)
                    hostBuilder.UseSsl();

                if (!string.IsNullOrEmpty(options.Username))
                    hostBuilder.UseUsername(options.Username);

                if (!string.IsNullOrEmpty(options.Password))
                    hostBuilder.UsePassword(options.Password);

                hostConfigurator?.Invoke(hostBuilder);
            });
        }

        /// <summary>
        /// Configures a ActiveMQ bus by using the specified application configuration.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>.
        /// <param name="configuration">The <see cref="IConfiguration"/> being bound.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the ActiveMQ bus.</param>
        public static void UseActiveMq(this IMassTransitBuilder builder, IConfiguration configuration, Action<IActiveMqHostBuilder> hostConfigurator = null)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var connectionName = configuration["ConnectionName"];
            var hostBuilder = new ActiveMqHostBuilder(builder.Services, connectionName, configuration);
            hostConfigurator?.Invoke(hostBuilder);
        }

        /// <summary>
        /// Configures a ActiveMQ bus by using the specified settings.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>.
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="host">The host name of the ActiveMQ broker.</param>
        /// <param name="port">The port to connect to on the ActiveMQ broker.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the ActiveMQ bus.</param>
        public static void UseActiveMq(this IMassTransitBuilder builder, string connectionName, string host, int? port, Action<IActiveMqHostBuilder> hostConfigurator = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            var hostBuilder = new ActiveMqHostBuilder(builder.Services, connectionName, host, port);
            hostConfigurator?.Invoke(hostBuilder);
        }


    }
}