using System;

namespace MassTransit.Extensions.Hosting.RabbitMq
{
    /// <summary>
    /// Provides extension methods for <see cref="IRabbitMqHostBuilder"/>.
    /// </summary>
    public static class RabbitMqHostBuilderExtensions
    {
        /// <summary>
        /// Sets the username and password for the connection to RabbitMq.
        /// </summary>
        /// <param name="builder"><see cref="IRabbitMqHostBuilder"/></param>
        /// <param name="username">Contains the username.</param>
        /// <param name="password">Contains the password.</param>
        public static IRabbitMqHostBuilder UseCredentials(this IRabbitMqHostBuilder builder, string username, string password)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.UseUsername(username);
            builder.UsePassword(password);

            return builder;
        }

        /// <summary>
        /// Adds a configuration callback to the builder that is used to configure
        /// a receiving endpoint for the Bus with the specified queue name.
        /// </summary>
        /// <param name="builder"><see cref="IRabbitMqHostBuilder"/></param>
        /// <param name="queueName">The queue name for the receiving endpoint.</param>
        /// <param name="endpointConfigurator">The configuration callback to configure the receiving endpoint.</param>
        public static void AddReceiveEndpoint(this IRabbitMqHostBuilder builder, string queueName, Action<IRabbitMqReceiveEndpointBuilder> endpointConfigurator)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (queueName == null)
                throw new ArgumentNullException(nameof(queueName));

            var endpointBuilder = new RabbitMqReceiveEndpointBuilder(builder.Services);
            endpointConfigurator?.Invoke(endpointBuilder);

            builder.AddConfigurator((host, busFactory, serviceProvider) =>
            {
                busFactory.ReceiveEndpoint(host, queueName, endpoint =>
                {
                    endpointBuilder.Configure(host, endpoint, serviceProvider);
                });
            });
        }

    }
}