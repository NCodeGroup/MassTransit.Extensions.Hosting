using System;
using MassTransit.ConsumeConfigurators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MassTransit.Extensions.Hosting
{
    /// <summary>
    /// Provides extension methods for <see cref="IReceiveEndpointBuilder"/>.
    /// </summary>
    public static class ReceiveEndpointBuilderExtensions
    {
        /// <summary>
        /// Adds a configuration callback to the builder that is used to configure the receiving endpoint.
        /// </summary>
        /// <param name="builder"><see cref="IReceiveEndpointBuilder{THost,TEndpoint}"/></param>
        /// <param name="endpointConfigurator">The configuration callback to configure the receiving endpoint.</param>
        public static void AddConfigurator<THost, TEndpoint>(this IReceiveEndpointBuilder<THost, TEndpoint> builder, Action<TEndpoint> endpointConfigurator)
            where THost : class, IHost
            where TEndpoint : class, IReceiveEndpointConfigurator
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (endpointConfigurator == null)
                throw new ArgumentNullException(nameof(endpointConfigurator));

            builder.AddConfigurator((host, endpoint, serviceProvider) => endpointConfigurator(endpoint));
        }

        /// <summary>
        /// Adds a configuration callback to the builder that is used to configure the receiving endpoint.
        /// </summary>
        /// <param name="builder"><see cref="IReceiveEndpointBuilder{THost,TEndpoint}"/></param>
        /// <param name="endpointConfigurator">The configuration callback to configure the receiving endpoint.</param>
        public static void AddConfigurator<THost, TEndpoint>(this IReceiveEndpointBuilder<THost, TEndpoint> builder, Action<THost, TEndpoint> endpointConfigurator)
            where THost : class, IHost
            where TEndpoint : class, IReceiveEndpointConfigurator
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (endpointConfigurator == null)
                throw new ArgumentNullException(nameof(endpointConfigurator));

            builder.AddConfigurator((host, endpoint, serviceProvider) => endpointConfigurator(host, endpoint));
        }

        /// <summary>
        /// Adds a configuration callback to the builder that is used to configure the receiving endpoint.
        /// </summary>
        /// <param name="builder"><see cref="IReceiveEndpointBuilder{THost,TEndpoint}"/></param>
        /// <param name="endpointConfigurator">The configuration callback to configure the receiving endpoint.</param>
        public static void AddConfigurator<THost, TEndpoint>(this IReceiveEndpointBuilder<THost, TEndpoint> builder, Action<TEndpoint, IServiceProvider> endpointConfigurator)
            where THost : class, IHost
            where TEndpoint : class, IReceiveEndpointConfigurator
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (endpointConfigurator == null)
                throw new ArgumentNullException(nameof(endpointConfigurator));

            builder.AddConfigurator((host, endpoint, serviceProvider) => endpointConfigurator(endpoint, serviceProvider));
        }

        /// <summary>
        /// Adds a configuration callback to the builder that is used to connect a consumer to the receiving endpoint.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <param name="builder"><see cref="IReceiveEndpointBuilder"/></param>
        public static void AddConsumer<TConsumer>(this IReceiveEndpointBuilder builder)
            where TConsumer : class, IConsumer
        {
            AddConsumer<TConsumer>(builder, (consumerConfigure, serviceProvider) => { });
        }

        /// <summary>
        /// Adds a configuration callback to the builder that is used to connect a consumer to the receiving endpoint.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <param name="builder"><see cref="IReceiveEndpointBuilder"/></param>
        /// <param name="consumerConfigurator">The configuration callback to configure the consumer.</param>
        public static void AddConsumer<TConsumer>(this IReceiveEndpointBuilder builder, Action<IConsumerConfigurator<TConsumer>> consumerConfigurator)
            where TConsumer : class, IConsumer
        {
            AddConsumer<TConsumer>(builder, (consumerConfigure, serviceProvider) => consumerConfigurator(consumerConfigure));
        }

        /// <summary>
        /// Adds a configuration callback to the builder that is used to connect a consumer to the receiving endpoint.
        /// </summary>
        /// <typeparam name="TConsumer">The type of the consumer.</typeparam>
        /// <param name="builder"><see cref="IReceiveEndpointBuilder"/></param>
        /// <param name="consumerConfigurator">The configuration callback to configure the consumer.</param>
        public static void AddConsumer<TConsumer>(this IReceiveEndpointBuilder builder, Action<IConsumerConfigurator<TConsumer>, IServiceProvider> consumerConfigurator)
            where TConsumer : class, IConsumer
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddTransient<TConsumer>();

            builder.AddConfigurator((host, endpoint, serviceProvider) =>
            {
                var consumerFactory = serviceProvider.GetRequiredService<IConsumerFactory<TConsumer>>();
                endpoint.Consumer(consumerFactory, consumerConfigure => consumerConfigurator?.Invoke(consumerConfigure, serviceProvider));
            });
        }

    }
}