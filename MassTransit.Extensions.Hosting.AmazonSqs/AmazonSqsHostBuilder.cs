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
using System.Collections.Generic;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using MassTransit.AmazonSqsTransport;
using MassTransit.AmazonSqsTransport.Configuration;
using MassTransit.AmazonSqsTransport.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MassTransit.Extensions.Hosting.AmazonSqs
{
    /// <summary>
    /// Provides an abstraction to configure and initialize AmazonSQS specific Bus instances.
    /// </summary>
    public interface IAmazonSqsHostBuilder : IBusHostBuilder<IAmazonSqsHost, IAmazonSqsBusFactoryConfigurator>
    {
        /// <summary>
        /// Sets the AWS region to connect to.
        /// </summary>
        /// <param name="region">The new value.</param>
        /// <returns><see cref="IAmazonSqsHostBuilder"/></returns>
        IAmazonSqsHostBuilder UseRegion(RegionEndpoint region);

        /// <summary>
        /// Sets the credentials for AWS.
        /// </summary>
        /// <param name="credentials">The new value.</param>
        /// <returns><see cref="IAmazonSqsHostBuilder"/></returns>
        IAmazonSqsHostBuilder UseCredentials(AWSCredentials credentials);

        /// <summary>
        /// Sets the SQS configuration for AWS.
        /// </summary>
        /// <param name="config">The new value.</param>
        /// <returns><see cref="IAmazonSqsHostBuilder"/></returns>
        IAmazonSqsHostBuilder UseConfig(AmazonSQSConfig config);

        /// <summary>
        /// Sets the SNS configuration for AWS.
        /// </summary>
        /// <param name="config">The new value.</param>
        /// <returns><see cref="IAmazonSqsHostBuilder"/></returns>
        IAmazonSqsHostBuilder UseConfig(AmazonSimpleNotificationServiceConfig config);
    }

    /// <summary>
    /// Provides an implementation of <see cref="IAmazonSqsHostBuilder"/>.
    /// </summary>
    public class AmazonSqsHostBuilder :
        BusHostBuilder<IAmazonSqsHost, IAmazonSqsBusFactoryConfigurator>,
        IAmazonSqsHostBuilder
    {
        private readonly IList<Action<AmazonSqsHostConfigurator>> _hostConfiguratorActions = new List<Action<AmazonSqsHostConfigurator>>();
        private readonly Func<IServiceProvider, AmazonSqsHostConfigurator> _hostConfiguratorFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AmazonSqsHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        public AmazonSqsHostBuilder(IServiceCollection services, string connectionName)
            : base(services, connectionName)
        {
            _hostConfiguratorFactory = serviceProvider => new AmazonSqsHostConfigurator();

            services.TryAddTransient<IBusFactory<IAmazonSqsBusFactoryConfigurator>, AmazonSqsBusFactory>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AmazonSqsHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="hostAddress">The URI host address of the AWS region (example: amazonsqs://regionSystemName/).</param>
        public AmazonSqsHostBuilder(IServiceCollection services, string connectionName, Uri hostAddress)
            : base(services, connectionName)
        {
            if (hostAddress == null)
                throw new ArgumentNullException(nameof(hostAddress));
            if (!string.Equals("amazonsqs", hostAddress.Scheme, StringComparison.OrdinalIgnoreCase))
                throw new AmazonSqsTransportConfigurationException($"The address scheme was invalid: {hostAddress.Scheme}");

            var region = RegionEndpoint.GetBySystemName(hostAddress.Host);
            if (!string.IsNullOrEmpty(hostAddress.UserInfo))
            {
                var parts = hostAddress.UserInfo.Split(':');
                if (parts.Length >= 2)
                {
                    var accessKey = parts[0];
                    var secretKey = parts[1];

                    _hostConfiguratorActions.Add(config => config.AccessKey(accessKey));
                    _hostConfiguratorActions.Add(config => config.SecretKey(secretKey));
                }
            }

            _hostConfiguratorFactory = serviceProvider => new AmazonSqsHostConfigurator(region);

            services.TryAddTransient<IBusFactory<IAmazonSqsBusFactoryConfigurator>, AmazonSqsBusFactory>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AmazonSqsHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="region">The AWS region to connect to.</param>
        public AmazonSqsHostBuilder(IServiceCollection services, string connectionName, RegionEndpoint region)
            : base(services, connectionName)
        {
            _hostConfiguratorFactory = serviceProvider => new AmazonSqsHostConfigurator(region);

            services.TryAddTransient<IBusFactory<IAmazonSqsBusFactoryConfigurator>, AmazonSqsBusFactory>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AmazonSqsHostBuilder"/> class.
        /// </summary>
        /// <param name="services"><see cref="IServiceCollection"/></param>
        /// <param name="connectionName">The client-provided connection name.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> being bound.</param>
        public AmazonSqsHostBuilder(IServiceCollection services, string connectionName, IConfiguration configuration)
            : base(services, connectionName)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            services.AddOptions();
            services.Configure<AmazonSqsOptions>(ConnectionName, configuration);
            services.TryAddTransient<IBusFactory<IAmazonSqsBusFactoryConfigurator>, AmazonSqsBusFactory>();

            _hostConfiguratorFactory = serviceProvider =>
            {
                var optionsSnapshot = serviceProvider.GetRequiredService<IOptionsSnapshot<AmazonSqsOptions>>();
                var options = optionsSnapshot.Get(ConnectionName);

                var hostConfigurator = new AmazonSqsHostConfigurator();

                if (!string.IsNullOrEmpty(options.RegionSystemName))
                {
                    var region = RegionEndpoint.GetBySystemName(options.RegionSystemName);
                    hostConfigurator.Region(region);
                }

                var credentials = options.GetCredentials();
                if (credentials != null)
                    hostConfigurator.Credentials(credentials);

                if (options.SqsConfig != null)
                    hostConfigurator.Config(options.SqsConfig);

                if (options.SnsConfig != null)
                    hostConfigurator.Config(options.SnsConfig);

                return hostConfigurator;
            };
        }

        /// <inheritdoc />
        public virtual IAmazonSqsHostBuilder UseRegion(RegionEndpoint region)
        {
            _hostConfiguratorActions.Add(configure => configure.Region(region));
            return this;
        }

        /// <inheritdoc />
        public virtual IAmazonSqsHostBuilder UseCredentials(AWSCredentials credentials)
        {
            _hostConfiguratorActions.Add(configure => configure.Credentials(credentials));
            return this;
        }

        /// <inheritdoc />
        public virtual IAmazonSqsHostBuilder UseConfig(AmazonSQSConfig config)
        {
            _hostConfiguratorActions.Add(configure => configure.Config(config));
            return this;
        }

        /// <inheritdoc />
        public virtual IAmazonSqsHostBuilder UseConfig(AmazonSimpleNotificationServiceConfig config)
        {
            _hostConfiguratorActions.Add(configure => configure.Config(config));
            return this;
        }

        /// <inheritdoc />
        public override IBusControl Create(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException(nameof(serviceProvider));

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger<AmazonSqsHostBuilder>();
            var loggerIsEnabled = logger?.IsEnabled(LogLevel.Debug) ?? false;
            if (loggerIsEnabled)
                logger.LogDebug("Creating AmazonSQS Bus '{0}'", ConnectionName);

            var busFactory = serviceProvider.GetRequiredService<IBusFactory<IAmazonSqsBusFactoryConfigurator>>();
            var busControl = busFactory.Create(busFactoryConfigurator =>
            {
                var hostConfigurator = _hostConfiguratorFactory(serviceProvider);
                foreach (var hostConfiguratorAction in _hostConfiguratorActions)
                {
                    hostConfiguratorAction(hostConfigurator);
                }

                var settings = hostConfigurator.CreateSettings();
                var host = busFactoryConfigurator.Host(settings);

                Configure(host, busFactoryConfigurator, serviceProvider);
            });

            if (loggerIsEnabled)
                logger.LogDebug("Created AmazonSQS Bus '{0}'", ConnectionName);

            return busControl;
        }

    }
}