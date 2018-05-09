using System;

namespace MassTransit.Extensions.Hosting
{
    /// <summary>
    /// Provides extension methods for <see cref="IMassTransitBuilder"/> to configure InMemory bus instances.
    /// </summary>
    public static class UseInMemoryExtensions
    {
        /// <summary>
        /// Configures an InMemory bus.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the InMemory bus.</param>
        public static void UseInMemory(this IMassTransitBuilder builder, string connectionName, Action<IInMemoryHostBuilder> hostConfigurator)
        {
            UseInMemory(builder, connectionName, null, hostConfigurator);
        }

        /// <summary>
        /// Configures an InMemory bus by using the specified base address.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>.
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="baseAddress">Contains an optional override for the default base address.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the InMemory bus.</param>
        public static void UseInMemory(this IMassTransitBuilder builder, string connectionName, Uri baseAddress, Action<IInMemoryHostBuilder> hostConfigurator)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            var hostBuilder = new InMemoryHostBuilder(builder.Services, connectionName, baseAddress);
            hostConfigurator?.Invoke(hostBuilder);
        }

    }
}