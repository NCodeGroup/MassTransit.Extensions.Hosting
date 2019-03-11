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
using System.Threading.Tasks;
using MassTransit.RabbitMqTransport;
using MassTransit.RabbitMqTransport.Configurators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MassTransit.Extensions.Hosting.RabbitMq
{
    /// <summary>
    /// Provides an abstraction to configure and initialize RabbitMq specific Bus instances.
    /// </summary>
    public interface IRabbitMqHostBuilder : IBusHostBuilder<IRabbitMqHost, IRabbitMqBusFactoryConfigurator>
    {
        /// <summary>
        /// Sets the username for the connection to RabbitMq.
        /// </summary>
        /// <param name="username">The new value.</param>
        /// <returns><see cref="IRabbitMqHostBuilder"/></returns>
        IRabbitMqHostBuilder UseUsername(string username);

        /// <summary>
        /// Sets the password for the connection to RabbitMq.
        /// </summary>
        /// <param name="password">The new value.</param>
        /// <returns><see cref="IRabbitMqHostBuilder"/></returns>
        IRabbitMqHostBuilder UsePassword(string password);

        /// <summary>
        /// Specifies the heartbeat interval, in seconds, used to maintain the
        /// connection to RabbitMq. Setting this value to zero will disable
        /// heartbeats, allowing the connection to timeout after an inactivity
        /// period.
        /// </summary>
        /// <param name="heartbeat">The new value.</param>
        /// <returns><see cref="IRabbitMqHostBuilder"/></returns>
        IRabbitMqHostBuilder UseHeartbeat(ushort heartbeat);

        /// <summary>
        /// Enables RabbitMq publish acknowledgement, so that the <see cref="Task"/>
        /// returned from Send/Publish is not completed until the message has
        /// been confirmed by the broker.
        /// </summary>
        /// <param name="enable">The new value.</param>
        /// <returns><see cref="IRabbitMqHostBuilder"/></returns>
        IRabbitMqHostBuilder UsePublisherConfirmation(bool enable = true);

        /// <summary>
        /// Configure the use of SSL to connection to RabbitMq.
        /// </summary>
        /// <param name="configureSsl">The configuration callback to configure SSL.</param>
        /// <returns><see cref="IRabbitMqHostBuilder"/></returns>
        IRabbitMqHostBuilder UseSsl(Action<IRabbitMqSslConfigurator> configureSsl);

        /// <summary>
        /// Configure a RabbitMq High-Availability cluster which will cycle
        /// hosts when connections are interrupted.
        /// </summary>
        /// <param name="configureCluster">The configuration callback to configure the cluster.</param>
        /// <returns><see cref="IRabbitMqHostBuilder"/></returns>
        IRabbitMqHostBuilder UseCluster(Action<IRabbitMqClusterConfigurator> configureCluster);
    }

    /// <summary>
    /// Provides an implementation of <see cref="IRabbitMqHostBuilder"/>.
    /// </summary>
    public class RabbitMqHostBuilder :
        BusHostBuilder<IRabbitMqHost, IRabbitMqBusFactoryConfigurator>,
        IRabbitMqHostBuilder
    {
        private readonly IList<Action<IRabbitMqHostConfigurator>> _hostConfiguratorActions = new List<Action<IRabbitMqHostConfigurator>>();
        private readonly Func<IServiceProvider, RabbitMqHostConfigurator> _hostConfiguratorFactory;

        private RabbitMqHostBuilder(IServiceCollection services, string connectionName)
            : base(services, connectionName)
        {
            services.TryAddTransient<IBusFactory<IRabbitMqBusFactoryConfigurator>, RabbitMqBusFactory>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostAddress">The URI host address of the RabbitMQ host (example: rabbitmq://host:port/vhost).</param>
        public RabbitMqHostBuilder(IServiceCollection services, string connectionName, Uri hostAddress)
            : this(services, connectionName)
        {
            if (hostAddress == null)
                throw new ArgumentNullException(nameof(hostAddress));

            _hostConfiguratorFactory = serviceProvider => new RabbitMqHostConfigurator(hostAddress, connectionName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="host">The host name of the RabbitMq broker.</param>
        /// <param name="port">The port to connect to on the RabbitMq broker.</param>
        /// <param name="virtualHost">The virtual host to use.</param>
        public RabbitMqHostBuilder(IServiceCollection services, string connectionName, string host, int port, string virtualHost)
            : this(services, connectionName)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));
            if (virtualHost == null)
                throw new ArgumentNullException(nameof(virtualHost));

            _hostConfiguratorFactory = serviceProvider => new RabbitMqHostConfigurator(host, virtualHost, (ushort)port, connectionName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> being bound.</param>
        public RabbitMqHostBuilder(IServiceCollection services, string connectionName, IConfiguration configuration)
            : this(services, connectionName)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            services.AddOptions();
            services.Configure<RabbitMqOptions>(ConnectionName, configuration);

            _hostConfiguratorFactory = serviceProvider =>
            {
                var optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<RabbitMqOptions>>();
                var options = optionsSnapshot.Get(ConnectionName);

                var hostConfigurator = new RabbitMqHostConfigurator(options.HostAddress, ConnectionName);

                if (options.Heartbeat.HasValue)
                    hostConfigurator.Heartbeat(options.Heartbeat.Value);

                if (!string.IsNullOrEmpty(options.Username))
                    hostConfigurator.Username(options.Username);

                if (!string.IsNullOrEmpty(options.Password))
                    hostConfigurator.Password(options.Password);

                return hostConfigurator;
            };
        }

        /// <inheritdoc />
        public virtual IRabbitMqHostBuilder UseUsername(string username)
        {
            _hostConfiguratorActions.Add(configure => configure.Username(username));
            return this;
        }

        /// <inheritdoc />
        public virtual IRabbitMqHostBuilder UsePassword(string password)
        {
            _hostConfiguratorActions.Add(configure => configure.Password(password));
            return this;
        }

        /// <inheritdoc />
        public virtual IRabbitMqHostBuilder UseHeartbeat(ushort heartbeat)
        {
            _hostConfiguratorActions.Add(configure => configure.Heartbeat(heartbeat));
            return this;
        }

        /// <inheritdoc />
        public virtual IRabbitMqHostBuilder UsePublisherConfirmation(bool enable = true)
        {
            _hostConfiguratorActions.Add(configure => configure.PublisherConfirmation = enable);
            return this;
        }

        /// <inheritdoc />
        public virtual IRabbitMqHostBuilder UseSsl(Action<IRabbitMqSslConfigurator> configureSsl)
        {
            _hostConfiguratorActions.Add(configure => configure.UseSsl(configureSsl));
            return this;
        }

        /// <inheritdoc />
        public virtual IRabbitMqHostBuilder UseCluster(Action<IRabbitMqClusterConfigurator> configureCluster)
        {
            _hostConfiguratorActions.Add(configure => configure.UseCluster(configureCluster));
            return this;
        }

        /// <inheritdoc />
        public override IBusControl Create(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger<RabbitMqHostBuilder>();
            var loggerIsEnabled = logger?.IsEnabled(LogLevel.Debug) ?? false;
            if (loggerIsEnabled)
                logger.LogDebug("Creating RabbitMQ Bus '{0}'", ConnectionName);

            var busFactory = serviceProvider.GetRequiredService<IBusFactory<IRabbitMqBusFactoryConfigurator>>();
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
                logger.LogDebug("Created RabbitMQ Bus '{0}'", ConnectionName);

            return busControl;
        }

    }
}