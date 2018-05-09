using System;
using System.Threading.Tasks;
using MassTransit.RabbitMqTransport;
using MassTransit.RabbitMqTransport.Configurators;
using Microsoft.Extensions.DependencyInjection;

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
        private readonly RabbitMqHostConfigurator _hostConfigurator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostAddress">The URI host address of the RabbitMQ host (example: rabbitmq://host:port/vhost).</param>
        public RabbitMqHostBuilder(IServiceCollection services, string connectionName, Uri hostAddress)
            : base(services, connectionName)
        {
            if (hostAddress == null)
                throw new ArgumentNullException(nameof(hostAddress));

            _hostConfigurator = new RabbitMqHostConfigurator(hostAddress, connectionName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="host">The host name of the RabbitMq broker.</param>
        /// <param name="port">The port to connect to on the RabbitMq broker.</param>
        /// <param name="virtualHost">The virtual host to use.</param>
        public RabbitMqHostBuilder(IServiceCollection services, string connectionName, string host, ushort port, string virtualHost)
            : base(services, connectionName)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));
            if (virtualHost == null)
                throw new ArgumentNullException(nameof(virtualHost));

            _hostConfigurator = new RabbitMqHostConfigurator(host, virtualHost, port, connectionName);
        }

        /// <inheritdoc />
        public virtual IRabbitMqHostBuilder UseUsername(string username)
        {
            _hostConfigurator.Username(username);
            return this;
        }

        /// <inheritdoc />
        public virtual IRabbitMqHostBuilder UsePassword(string password)
        {
            _hostConfigurator.Password(password);
            return this;
        }

        /// <inheritdoc />
        public virtual IRabbitMqHostBuilder UseHeartbeat(ushort heartbeat)
        {
            _hostConfigurator.Heartbeat(heartbeat);
            return this;
        }

        /// <inheritdoc />
        public virtual IRabbitMqHostBuilder UsePublisherConfirmation(bool enable = true)
        {
            _hostConfigurator.PublisherConfirmation = enable;
            return this;
        }

        /// <inheritdoc />
        public virtual IRabbitMqHostBuilder UseSsl(Action<IRabbitMqSslConfigurator> configureSsl)
        {
            _hostConfigurator.UseSsl(configureSsl);
            return this;
        }

        /// <inheritdoc />
        public virtual IRabbitMqHostBuilder UseCluster(Action<IRabbitMqClusterConfigurator> configureCluster)
        {
            _hostConfigurator.UseCluster(configureCluster);
            return this;
        }

        /// <inheritdoc />
        public override IBusControl Create(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var busControl = Bus.Factory.CreateUsingRabbitMq(busFactory =>
            {
                var host = busFactory.Host(_hostConfigurator.Settings);

                Configure(host, busFactory, serviceProvider);
            });

            return busControl;
        }

    }
}