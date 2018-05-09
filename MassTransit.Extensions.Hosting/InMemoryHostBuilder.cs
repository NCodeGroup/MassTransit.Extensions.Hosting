using System;
using MassTransit.Transports.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace MassTransit.Extensions.Hosting
{
    /// <summary>
    /// Provides an abstraction to configure and initialize InMemory specific Bus instances.
    /// </summary>
    public interface IInMemoryHostBuilder : IBusHostBuilder<IInMemoryHost, IInMemoryBusFactoryConfigurator>
    {
        // nothing
    }

    /// <summary>
    /// Provides an implementation of <see cref="IInMemoryHostBuilder"/>.
    /// </summary>
    public class InMemoryHostBuilder :
        BusHostBuilder<IInMemoryHost, IInMemoryBusFactoryConfigurator>,
        IInMemoryHostBuilder
    {
        private readonly Uri _baseAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="baseAddress">Contains an optional override for the default base address.</param>
        public InMemoryHostBuilder(IServiceCollection services, string connectionName, Uri baseAddress)
            : base(services, connectionName)
        {
            _baseAddress = baseAddress;
        }

        /// <inheritdoc />
        public override IBusControl Create(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var busControl = Bus.Factory.CreateUsingInMemory(_baseAddress, busFactory =>
            {
                Configure(busFactory.Host, busFactory, serviceProvider);
            });

            return busControl;
        }

    }
}