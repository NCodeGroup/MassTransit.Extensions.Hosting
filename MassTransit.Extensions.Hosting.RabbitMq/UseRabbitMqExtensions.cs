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
        /// <param name="hostAddress">The URI host address of the RabbitMQ host (example: rabbitmq://host:port/vhost).</param>
        /// <param name="hostConfigurator">The configuration callback to configure the RabbitMq bus.</param>
        public static void UseRabbitMq(this IMassTransitBuilder builder, Uri hostAddress, Action<IRabbitMqHostBuilder> hostConfigurator)
        {
            UseRabbitMq(builder, hostAddress, null, hostConfigurator);
        }

        /// <summary>
        /// Configures a RabbitMq bus.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="hostAddress">The URI host address of the RabbitMQ host (example: rabbitmq://host:port/vhost).</param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the RabbitMq bus.</param>
        public static void UseRabbitMq(this IMassTransitBuilder builder, Uri hostAddress, string connectionName, Action<IRabbitMqHostBuilder> hostConfigurator)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (hostAddress == null)
                throw new ArgumentNullException(nameof(hostAddress));

            var hostBuilder = new RabbitMqHostBuilder(builder.Services, hostAddress, connectionName);
            hostConfigurator?.Invoke(hostBuilder);
        }

        /// <summary>
        /// Configures a RabbitMq bus.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="host">The host name of the RabbitMq broker.</param>
        /// <param name="virtualHost">The virtual host to use.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the RabbitMq bus.</param>
        public static void UseRabbitMq(this IMassTransitBuilder builder, string host, string virtualHost, Action<IRabbitMqHostBuilder> hostConfigurator)
        {
            UseRabbitMq(builder, host, virtualHost, null, hostConfigurator);
        }

        /// <summary>
        /// Configures a RabbitMq bus.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="host">The host name of the RabbitMq broker.</param>
        /// <param name="virtualHost">The virtual host to use.</param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the RabbitMq bus.</param>
        public static void UseRabbitMq(this IMassTransitBuilder builder, string host, string virtualHost, string connectionName, Action<IRabbitMqHostBuilder> hostConfigurator)
        {
            UseRabbitMq(builder, host, RabbitMqOptions.DefaultPort, virtualHost, connectionName, hostConfigurator);
        }

        /// <summary>
        /// Configures a RabbitMq bus.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="host">The host name of the RabbitMq broker.</param>
        /// <param name="port">The port to connect to on the RabbitMq broker.</param>
        /// <param name="virtualHost">The virtual host to use.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the RabbitMq bus.</param>
        public static void UseRabbitMq(this IMassTransitBuilder builder, string host, ushort port, string virtualHost, Action<IRabbitMqHostBuilder> hostConfigurator)
        {
            UseRabbitMq(builder, host, port, virtualHost, null, hostConfigurator);
        }

        /// <summary>
        /// Configures a RabbitMq bus by using the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>.
        /// <param name="options"><see cref="RabbitMqOptions"/></param>
        /// <param name="hostConfigurator">The configuration callback to configure the RabbitMq bus.</param>
        public static void UseRabbitMq(this IMassTransitBuilder builder, RabbitMqOptions options, Action<IRabbitMqHostBuilder> hostConfigurator)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            UseRabbitMq(builder, options.Host, options.Port, options.VirtualHost, options.ConnectionName, hostBuilder =>
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
        /// <param name="host">The host name of the RabbitMq broker.</param>
        /// <param name="port">The port to connect to on the RabbitMq broker.</param>
        /// <param name="virtualHost">The virtual host to use.</param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the RabbitMq bus.</param>
        public static void UseRabbitMq(this IMassTransitBuilder builder, string host, ushort port, string virtualHost, string connectionName, Action<IRabbitMqHostBuilder> hostConfigurator)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (host == null)
                throw new ArgumentNullException(nameof(host));
            if (virtualHost == null)
                throw new ArgumentNullException(nameof(virtualHost));

            var hostBuilder = new RabbitMqHostBuilder(builder.Services, host, port, virtualHost, connectionName);
            hostConfigurator?.Invoke(hostBuilder);
        }

    }
}