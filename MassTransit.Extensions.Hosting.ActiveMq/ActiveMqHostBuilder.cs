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
using MassTransit.ActiveMqTransport;
using MassTransit.ActiveMqTransport.Configurators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MassTransit.Extensions.Hosting.ActiveMq
{
    /// <summary>
    /// Provides an abstraction to configure and initialize ActiveMQ specific Bus instances.
    /// </summary>
    public interface IActiveMqHostBuilder : IBusHostBuilder<IActiveMqHost, IActiveMqBusFactoryConfigurator>
    {
        /// <summary>
        /// Sets the username for the connection to ActiveMQ.
        /// </summary>
        /// <param name="username">The new value.</param>
        /// <returns><see cref="IActiveMqHostBuilder"/></returns>
        IActiveMqHostBuilder UseUsername(string username);

        /// <summary>
        /// Sets the password for the connection to ActiveMQ.
        /// </summary>
        /// <param name="password">The new value.</param>
        /// <returns><see cref="IActiveMqHostBuilder"/></returns>
        IActiveMqHostBuilder UsePassword(string password);

        /// <summary>
        /// Configure the use of SSL when connecting to ActiveMQ.
        /// </summary>
        /// <returns><see cref="IActiveMqHostBuilder"/></returns>
        IActiveMqHostBuilder UseSsl();
    }

    /// <summary>
    /// Provides an implementation of <see cref="IActiveMqHostBuilder"/>.
    /// </summary>
    public class ActiveMqHostBuilder :
        BusHostBuilder<IActiveMqHost, IActiveMqBusFactoryConfigurator>,
        IActiveMqHostBuilder
    {
        private readonly IList<Action<IActiveMqHostConfigurator>> _hostConfiguratorActions = new List<Action<IActiveMqHostConfigurator>>();
        private readonly Func<IServiceProvider, ActiveMqHostConfigurator> _hostConfiguratorFactory;

        private ActiveMqHostBuilder(IServiceCollection services, string connectionName)
            : base(services, connectionName)
        {
            services.TryAddTransient<IBusFactory<IActiveMqBusFactoryConfigurator>, ActiveMqBusFactory>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveMqHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostAddress">The URI host address of the ActiveMQ host (example: activemq://host:port/).</param>
        public ActiveMqHostBuilder(IServiceCollection services, string connectionName, Uri hostAddress)
            : this(services, connectionName)
        {
            if (hostAddress == null)
                throw new ArgumentNullException(nameof(hostAddress));

            _hostConfiguratorFactory = serviceProvider => new ActiveMqHostConfigurator(hostAddress);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveMqHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="host">The host name of the ActiveMQ broker.</param>
        /// <param name="port">The port to connect to on the ActiveMQ broker.</param>
        public ActiveMqHostBuilder(IServiceCollection services, string connectionName, string host, int? port = null)
            : this(services, connectionName)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            var hostAddress = ActiveMqOptions.FormatHostAddress(host, port, false);

            _hostConfiguratorFactory = serviceProvider => new ActiveMqHostConfigurator(hostAddress);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveMqHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> being bound.</param>
        public ActiveMqHostBuilder(IServiceCollection services, string connectionName, IConfiguration configuration)
            : this(services, connectionName)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            services.AddOptions();
            services.Configure<ActiveMqOptions>(ConnectionName, configuration);

            _hostConfiguratorFactory = serviceProvider =>
            {
                var optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<ActiveMqOptions>>();
                var options = optionsSnapshot.Get(ConnectionName);

                var hostConfigurator = new ActiveMqHostConfigurator(options.HostAddress);

                if (options.UseSsl)
                    hostConfigurator.UseSsl();

                if (!string.IsNullOrEmpty(options.Username))
                    hostConfigurator.Username(options.Username);

                if (!string.IsNullOrEmpty(options.Password))
                    hostConfigurator.Password(options.Password);

                return hostConfigurator;
            };
        }

        /// <inheritdoc />
        public virtual IActiveMqHostBuilder UseUsername(string username)
        {
            _hostConfiguratorActions.Add(configure => configure.Username(username));
            return this;
        }

        /// <inheritdoc />
        public virtual IActiveMqHostBuilder UsePassword(string password)
        {
            _hostConfiguratorActions.Add(configure => configure.Password(password));
            return this;
        }

        /// <inheritdoc />
        public virtual IActiveMqHostBuilder UseSsl()
        {
            _hostConfiguratorActions.Add(configure => configure.UseSsl());
            return this;
        }

        /// <inheritdoc />
        public override IBusControl Create(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger<ActiveMqHostBuilder>();
            var loggerIsEnabled = logger?.IsEnabled(LogLevel.Debug) ?? false;
            if (loggerIsEnabled)
                logger.LogDebug("Creating ActiveMQ Bus '{0}'", ConnectionName);

            var busFactory = serviceProvider.GetRequiredService<IBusFactory<IActiveMqBusFactoryConfigurator>>();
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
                logger.LogDebug("Created ActiveMQ Bus '{0}'", ConnectionName);

            return busControl;
        }

    }
}