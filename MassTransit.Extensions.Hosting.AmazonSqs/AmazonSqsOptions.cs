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

using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;

namespace MassTransit.Extensions.Hosting.AmazonSqs
{
    /// <summary>
    /// Contains the options for configuring a AmazonSQS host.
    /// </summary>
    public class AmazonSqsOptions
    {
        private AWSCredentials _credentials;

        /// <summary>
        /// Gets or sets the client-provided connection name.
        /// </summary>
        public string ConnectionName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the AWS region to connect to. The default value is <c>us-east-1</c>.
        /// </summary>
        public string RegionSystemName { get; set; } = RegionEndpoint.USEast1.SystemName;

        /// <summary>
        /// Gets or sets the AccessKey credential for AWS.
        /// </summary>
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets the SecretKey credential for AWS.
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// Gets or sets the AWS configuration for SQS.
        /// </summary>
        public AmazonSQSConfig SqsConfig { get; set; }

        /// <summary>
        /// Gets or sets the AWS configuration for SNS.
        /// </summary>
        public AmazonSimpleNotificationServiceConfig SnsConfig { get; set; }

        /// <summary>
        /// Allows to specify credential types other than basic.
        /// </summary>
        public virtual void SetCredentials(AWSCredentials credentials)
        {
            _credentials = credentials;

            AccessKey = null;
            SecretKey = null;
        }

        /// <summary>
        /// Gets the configured credentials for AWS.
        /// </summary>
        public virtual AWSCredentials GetCredentials()
        {
            return _credentials ?? new BasicAWSCredentials(AccessKey, SecretKey);
        }

    }
}