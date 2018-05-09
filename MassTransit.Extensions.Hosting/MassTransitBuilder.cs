using System;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.Extensions.Hosting
{
    /// <summary>
    /// Provides an abstraction to configure and initialize Bus instances.
    /// </summary>
    public interface IMassTransitBuilder
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> Dependency Injection container.
        /// </summary>
        IServiceCollection Services { get; }
    }

    /// <summary>
    /// Provides an implementation of <see cref="IMassTransitBuilder"/>.
    /// </summary>
    public class MassTransitBuilder : IMassTransitBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MassTransitBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        public MassTransitBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <inheritdoc />
        public IServiceCollection Services { get; }
    }
}