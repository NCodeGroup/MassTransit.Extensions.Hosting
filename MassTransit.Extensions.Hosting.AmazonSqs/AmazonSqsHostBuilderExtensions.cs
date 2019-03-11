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
using Amazon.Runtime;

namespace MassTransit.Extensions.Hosting.AmazonSqs
{
    /// <summary>
    /// Provides extension methods for <see cref="IAmazonSqsHostBuilder"/>.
    /// </summary>
    public static class AmazonSqsHostBuilderExtensions
    {
        /// <summary>
        /// Sets the AWS region based on its system name like "us-west-1".
        /// </summary>
        /// <param name="builder"><see cref="IAmazonSqsHostBuilder"/></param>
        /// <param name="systemName">The system name of the service like "us-west-1".</param>
        /// <returns><see cref="IAmazonSqsHostBuilder"/></returns>
        public static IAmazonSqsHostBuilder UseRegion(this IAmazonSqsHostBuilder builder, string systemName)
        {
            var region = RegionEndpoint.GetBySystemName(systemName);
            return builder.UseRegion(region);
        }

        /// <summary>
        /// Sets the specified AccessKey and SecretKey as the credentials to AWS.
        /// </summary>
        /// <param name="builder"><see cref="IAmazonSqsHostBuilder"/></param>
        /// <param name="accessKey">Contains the AccessKey credential for AWS.</param>
        /// <param name="secretKey">Contains the SecretKey credential for AWS.</param>
        /// <returns><see cref="IAmazonSqsHostBuilder"/></returns>
        public static IAmazonSqsHostBuilder UseCredentials(this IAmazonSqsHostBuilder builder, string accessKey, string secretKey)
        {
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            return builder.UseCredentials(credentials);
        }

        /// <summary>
        /// Adds a configuration callback to the builder that is used to configure
        /// a receiving endpoint for the Bus with the specified queue name.
        /// </summary>
        /// <param name="builder"><see cref="IAmazonSqsHostBuilder"/></param>
        /// <param name="queueName">The queue name for the receiving endpoint.</param>
        /// <param name="endpointConfigurator">The configuration callback to configure the receiving endpoint.</param>
        public static void AddReceiveEndpoint(this IAmazonSqsHostBuilder builder, string queueName, Action<IAmazonSqsReceiveEndpointBuilder> endpointConfigurator)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (queueName == null)
                throw new ArgumentNullException(nameof(queueName));

            var endpointBuilder = new AmazonSqsReceiveEndpointBuilder(builder.Services);
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