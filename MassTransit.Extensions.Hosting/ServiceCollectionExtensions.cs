using System;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.Scoping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace MassTransit.Extensions.Hosting
{
    /// <summary>
    /// Provides extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures the services required for using <c>MassTransit</c>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> used to configure the DI container.</param>
        /// <param name="configurator">The configuration callback to configure the Bus.</param>
        /// <returns><see cref="IServiceCollection"/></returns>
        public static IServiceCollection AddMassTransit(this IServiceCollection services, Action<IMassTransitBuilder> configurator)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            var builder = new MassTransitBuilder(services);
            configurator(builder);

            services.TryAddSingleton<IBusManager, BusManager>();
            services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<IBusManager>());

            services.TryAddTransient<IConsumerScopeProvider, DependencyInjectionConsumerScopeProvider>();
            services.TryAdd(ServiceDescriptor.Transient(typeof(IConsumerFactory<>), typeof(ScopeConsumerFactory<>)));

            return services;
        }

    }
}