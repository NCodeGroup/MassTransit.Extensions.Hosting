#region Copyright Preamble

// 
//    Copyright @ 2019 NCode Group
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

#endregion

using System;
using Amazon;
using Microsoft.Extensions.Configuration;

namespace MassTransit.Extensions.Hosting.AmazonSqs
{
    /// <summary>
    /// Provides extension methods for <see cref="IMassTransitBuilder"/> to configure AmazonSQS bus instances.
    /// </summary>
    public static class UseAmazonSqsExtensions
    {
        /// <summary>
        /// Configures a AmazonSQS bus.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the AmazonSQS bus.</param>
        public static void UseAmazonSqs(this IMassTransitBuilder builder, string connectionName, Action<IAmazonSqsHostBuilder> hostConfigurator)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (hostConfigurator == null)
                throw new ArgumentNullException(nameof(hostConfigurator));

            var hostBuilder = new AmazonSqsHostBuilder(builder.Services, connectionName);
            hostConfigurator.Invoke(hostBuilder);
        }

        /// <summary>
        /// Configures a AmazonSQS bus.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostAddress">The URI host address of the AWS region (example: amazonsqs://regionSystemName/).</param>
        /// <param name="hostConfigurator">The configuration callback to configure the AmazonSQS bus.</param>
        public static void UseAmazonSqs(this IMassTransitBuilder builder, string connectionName, Uri hostAddress, Action<IAmazonSqsHostBuilder> hostConfigurator = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (hostAddress == null)
                throw new ArgumentNullException(nameof(hostAddress));

            var hostBuilder = new AmazonSqsHostBuilder(builder.Services, connectionName, hostAddress);
            hostConfigurator?.Invoke(hostBuilder);
        }

        /// <summary>
        /// Configures a AmazonSQS bus.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="region">The AWS region to connect to.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the AmazonSQS bus.</param>
        public static void UseAmazonSqs(this IMassTransitBuilder builder, string connectionName, RegionEndpoint region, Action<IAmazonSqsHostBuilder> hostConfigurator = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (hostConfigurator == null)
                throw new ArgumentNullException(nameof(hostConfigurator));

            var hostBuilder = new AmazonSqsHostBuilder(builder.Services, connectionName, region);
            hostConfigurator.Invoke(hostBuilder);
        }

        /// <summary>
        /// Configures a AmazonSQS bus by using the specified <paramref name="options"/>.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>.
        /// <param name="options"><see cref="AmazonSqsOptions"/></param>
        /// <param name="hostConfigurator">The configuration callback to configure the AmazonSQS bus.</param>
        public static void UseAmazonSqs(this IMassTransitBuilder builder, AmazonSqsOptions options, Action<IAmazonSqsHostBuilder> hostConfigurator = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            UseAmazonSqs(builder, options.ConnectionName, hostBuilder =>
            {
                if (!string.IsNullOrEmpty(options.RegionSystemName))
                {
                    var region = RegionEndpoint.GetBySystemName(options.RegionSystemName);
                    hostBuilder.UseRegion(region);
                }

                var credentials = options.GetCredentials();
                if (credentials != null)
                    hostBuilder.UseCredentials(credentials);

                if (options.SqsConfig != null)
                    hostBuilder.UseConfig(options.SqsConfig);

                if (options.SnsConfig != null)
                    hostBuilder.UseConfig(options.SnsConfig);

                hostConfigurator?.Invoke(hostBuilder);
            });
        }

        /// <summary>
        /// Configures a AmazonSQS bus by using the specified application configuration.
        /// </summary>
        /// <param name="builder"><see cref="IMassTransitBuilder"/></param>.
        /// <param name="configuration">The <see cref="IConfiguration"/> being bound.</param>
        /// <param name="hostConfigurator">The configuration callback to configure the AmazonSQS bus.</param>
        public static void UseAmazonSqs(this IMassTransitBuilder builder, IConfiguration configuration, Action<IAmazonSqsHostBuilder> hostConfigurator = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var connectionName = configuration["ConnectionName"];
            var hostBuilder = new AmazonSqsHostBuilder(builder.Services, connectionName, configuration);
            hostConfigurator?.Invoke(hostBuilder);
        }

    }
}