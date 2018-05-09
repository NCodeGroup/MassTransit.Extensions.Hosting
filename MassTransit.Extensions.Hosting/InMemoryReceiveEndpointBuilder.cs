using MassTransit.Transports.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.Extensions.Hosting
{
    /// <summary>
    /// Provides an abstraction to configure and initialize InMemory specific receiving endpoints.
    /// </summary>
    public interface IInMemoryReceiveEndpointBuilder : IReceiveEndpointBuilder<IInMemoryHost, IInMemoryReceiveEndpointConfigurator>
    {
        // nothing
    }

    /// <summary>
    /// Provides the implementation for <see cref="IInMemoryReceiveEndpointBuilder"/>.
    /// </summary>
    public class InMemoryReceiveEndpointBuilder :
        ReceiveEndpointBuilder<IInMemoryHost, IInMemoryReceiveEndpointConfigurator>,
        IInMemoryReceiveEndpointBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryReceiveEndpointBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        public InMemoryReceiveEndpointBuilder(IServiceCollection services)
            : base(services)
        {
            // nothing
        }

    }
}