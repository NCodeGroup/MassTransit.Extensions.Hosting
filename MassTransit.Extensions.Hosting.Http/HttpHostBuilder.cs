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
using System.Net.Http;
using MassTransit.HttpTransport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MassTransit.Extensions.Hosting.Http
{
    /// <summary>
    /// Provides an abstraction to configure and initialize HTTP bus instances.
    /// </summary>
    public interface IHttpHostBuilder : IBusHostBuilder<IHttpHost, IHttpBusFactoryConfigurator>
    {
        /// <summary>
        /// Set the HTTP method to use, defaults to POST.
        /// </summary>
        /// <param name="value">The new value.</param>
        /// <returns><see cref="IHttpHostBuilder"/></returns>
        IHttpHostBuilder UseMethod(HttpMethod value);
    }

    /// <summary>
    /// Provides an implementation of <see cref="IHttpHostBuilder"/>.
    /// </summary>
    public class HttpHostBuilder :
        BusHostBuilder<IHttpHost, IHttpBusFactoryConfigurator>,
        IHttpHostBuilder
    {
        private readonly IList<Action<HttpHostConfigurator>> _hostConfiguratorActions = new List<Action<HttpHostConfigurator>>();
        private readonly Func<IServiceProvider, HttpHostConfigurator> _hostConfiguratorFactory;

        private HttpHostBuilder(IServiceCollection services, string connectionName)
            : base(services, connectionName)
        {
            services.TryAddTransient<IBusFactory<IHttpBusFactoryConfigurator>, HttpBusFactory>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="scheme">Specifies the HTTP or HTTPS scheme.</param>
        /// <param name="host">The HTTP host to connect to (should be a valid hostname).</param>
        /// <param name="port">The HTTP port to connect to.</param>
        public HttpHostBuilder(IServiceCollection services, string connectionName, string scheme, string host, int port)
            : this(services, connectionName)
        {
            if (scheme == null)
                throw new ArgumentNullException(nameof(scheme));
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            _hostConfiguratorFactory = serviceProvider => new HttpHostConfigurator(scheme, host, port);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> being bound.</param>
        public HttpHostBuilder(IServiceCollection services, string connectionName, IConfiguration configuration)
            : this(services, connectionName)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            services.AddOptions();
            services.Configure<HttpOptions>(ConnectionName, configuration);

            _hostConfiguratorFactory = serviceProvider =>
            {
                var optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<HttpOptions>>();
                var options = optionsSnapshot.Get(ConnectionName);

                var hostConfigurator = new HttpHostConfigurator(options.Scheme, options.Host, options.Port)
                {
                    Method = ToHttpMethod(options.Method),
                };

                return hostConfigurator;
            };
        }

        internal static HttpMethod ToHttpMethod(string method)
        {
            switch ((method ?? string.Empty).ToUpperInvariant())
            {
                case "GET":
                    return HttpMethod.Get;

                case "PUT":
                    return HttpMethod.Put;

                case "POST":
                    return HttpMethod.Post;

                case "DELETE":
                    return HttpMethod.Delete;

                case "HEAD":
                    return HttpMethod.Head;

                case "OPTIONS":
                    return HttpMethod.Options;

                case "TRACE":
                    return HttpMethod.Trace;
            }

            return new HttpMethod(method);
        }

        /// <inheritdoc />
        public virtual IHttpHostBuilder UseMethod(HttpMethod value)
        {
            _hostConfiguratorActions.Add(configure => configure.Method = value);
            return this;
        }

        /// <inheritdoc />
        public override IBusControl Create(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger<HttpHostBuilder>();
            var loggerIsEnabled = logger?.IsEnabled(LogLevel.Debug) ?? false;
            if (loggerIsEnabled)
                logger.LogDebug("Creating HTTP bus '{0}'", ConnectionName);

            var busFactory = serviceProvider.GetRequiredService<IBusFactory<IHttpBusFactoryConfigurator>>();
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
                logger.LogDebug("Created HTTP bus '{0}'", ConnectionName);

            return busControl;
        }

    }
}