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
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace MassTransit.Extensions.Hosting.Http
{
    /// <summary>
    /// Provides extension methods for <see cref="IMassTransitBuilder"/> to configure HTTP bus instances.
    /// </summary>
    public static class UseHttpExtensions
    {
        /// <summary>
        /// Configures a HTTP host using a MassTransit host address.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostAddress">The address of the HTTP service bus in MassTransit format (http://host:port/).</param>
        /// <param name="hostConfigurator">The configuration callback to configure the HTTP bus.</param>
        public static void UseHttp(this IMassTransitBuilder builder, string connectionName, Uri hostAddress, Action<IHttpHostBuilder> hostConfigurator = null)
        {
            if (hostAddress == null)
                throw new ArgumentNullException(nameof(hostAddress));

            UseHttp(builder, connectionName, hostAddress.Scheme, hostAddress.Host, hostAddress.Port, null, hostConfigurator);
        }
        /// <summary>
        /// Configures a HTTP host using a MassTransit host address.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostAddress">The address of the HTTP service bus in MassTransit format (http://host:port/).</param>
        /// <param name="method">The HTTP method (i.e. verb) to use, defaults to POST.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the HTTP bus.</param>
        public static void UseHttp(this IMassTransitBuilder builder, string connectionName, Uri hostAddress, HttpMethod method, Action<IHttpHostBuilder> hostConfigurator = null)
        {
            if (hostAddress == null)
                throw new ArgumentNullException(nameof(hostAddress));

            UseHttp(builder, connectionName, hostAddress.Scheme, hostAddress.Host, hostAddress.Port, method, hostConfigurator);
        }

        /// <summary>
        /// Configures a HTTP host using a MassTransit host address.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="scheme">Contains either HTTP or HTTPS.</param>
        /// <param name="host">The HTTP host to connect to (should be a valid hostname).</param>
        /// <param name="port">The HTTP port to connect to.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the HTTP bus.</param>
        public static void UseHttp(this IMassTransitBuilder builder, string connectionName, string scheme, string host, int port, Action<IHttpHostBuilder> hostConfigurator = null)
        {
            UseHttp(builder, connectionName, scheme, host, port, null, hostConfigurator);
        }

        /// <summary>
        /// Configures a HTTP host using a MassTransit host address.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="scheme">Specifies the HTTP or HTTPS scheme.</param>
        /// <param name="host">The HTTP host to connect to (should be a valid hostname).</param>
        /// <param name="port">The HTTP port to connect to.</param>
        /// <param name="method">The HTTP method (i.e. verb) to use, defaults to POST.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the HTTP bus.</param>
        public static void UseHttp(this IMassTransitBuilder builder, string connectionName, string scheme, string host, int port, HttpMethod method, Action<IHttpHostBuilder> hostConfigurator = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (scheme == null)
                throw new ArgumentNullException(nameof(scheme));
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            var hostBuilder = new HttpHostBuilder(builder.Services, connectionName, scheme, host, port);

            if (method != null)
                hostBuilder.UseMethod(method);

            hostConfigurator?.Invoke(hostBuilder);
        }

        /// <summary>
        /// Configures a HTTP host by using the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>.
        /// <param name="options"><see cref="HttpOptions"/></param>
        /// <param name="hostConfigurator">The configuration callback to configure the HTTP bus.</param>
        public static void UseHttp(this IMassTransitBuilder builder, HttpOptions options, Action<IHttpHostBuilder> hostConfigurator = null)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var method = HttpHostBuilder.ToHttpMethod(options.Method);

            UseHttp(builder, options.ConnectionName, options.Scheme, options.Host, options.Port, method, hostConfigurator);
        }

        /// <summary>
        /// Configures a HTTP host by using the specified application configuration.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>.
        /// <param name="configuration">The <see cref="IConfiguration"/> being bound.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the HTTP bus.</param>
        public static void UseHttp(this IMassTransitBuilder builder, IConfiguration configuration, Action<IHttpHostBuilder> hostConfigurator = null)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var connectionName = configuration["ConnectionName"];
            var hostBuilder = new HttpHostBuilder(builder.Services, connectionName, configuration);
            hostConfigurator?.Invoke(hostBuilder);
        }

    }
}