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
using MassTransit.AmazonSqsTransport;
using MassTransit.AmazonSqsTransport.Configuration;
using MassTransit.AmazonSqsTransport.Configuration.Configurators;

namespace MassTransit.Extensions.Hosting.AmazonSqs
{
    internal class AmazonSqsHostConfigurator : IAmazonSqsHostConfigurator
    {
        private RegionEndpoint _region;
        private string _accessKey;
        private string _secretKey;
        private AWSCredentials _credentials;
        private AmazonSQSConfig _sqsConfig;
        private AmazonSimpleNotificationServiceConfig _snsConfig;

        public AmazonSqsHostConfigurator()
        {
            // nothing
        }

        public AmazonSqsHostConfigurator(RegionEndpoint region)
        {
            _region = region; // nullable
        }

        public AmazonSqsHostSettings CreateSettings()
        {
            var credentials = _credentials;
            if (!string.IsNullOrEmpty(_accessKey) && !string.IsNullOrEmpty(_secretKey))
            {
                credentials = new BasicAWSCredentials(_accessKey, _secretKey);
            }

            var region = _region ?? RegionEndpoint.USEast1;
            var immutableCredentials = credentials?.GetCredentials();

            return new ConfigurationHostSettings
            {
                Region = region,
                AccessKey = immutableCredentials?.AccessKey,
                SecretKey = immutableCredentials?.SecretKey,
                AmazonSqsConfig = _sqsConfig,
                AmazonSnsConfig = _snsConfig,
            };
        }

        public virtual void Region(RegionEndpoint region)
        {
            _region = region;
        }

        /// <inheritdoc />
        public virtual void AccessKey(string accessKey)
        {
            _accessKey = accessKey;
            _credentials = null;
        }

        /// <inheritdoc />
        public virtual void SecretKey(string secretKey)
        {
            _secretKey = secretKey;
            _credentials = null;
        }

        public virtual void Credentials(AWSCredentials credentials)
        {
            _credentials = credentials;
            _accessKey = null;
            _secretKey = null;
        }

        /// <inheritdoc />
        public virtual void Config(AmazonSQSConfig config)
        {
            _sqsConfig = config;
        }

        /// <inheritdoc />
        public virtual void Config(AmazonSimpleNotificationServiceConfig config)
        {
            _snsConfig = config;
        }

    }
}