using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.Extensions.Hosting.RabbitMq
{
    /// <summary>
    /// Provides an abstraction to configure and initialize RabbitMq specific receiving endpoints.
    /// </summary>
    public interface IRabbitMqReceiveEndpointBuilder : IReceiveEndpointBuilder<IRabbitMqHost, IRabbitMqReceiveEndpointConfigurator>
    {
        // nothing
    }

    /// <summary>
    /// Provides an implementation of <see cref="IRabbitMqReceiveEndpointBuilder"/>.
    /// </summary>
    public class RabbitMqReceiveEndpointBuilder :
        ReceiveEndpointBuilder<IRabbitMqHost, IRabbitMqReceiveEndpointConfigurator>,
        IRabbitMqReceiveEndpointBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqReceiveEndpointBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        public RabbitMqReceiveEndpointBuilder(IServiceCollection services)
            : base(services)
        {
            // nothing
        }

    }
}