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
using Xunit;

namespace MassTransit.Extensions.Hosting.AmazonSqs.Tests
{
    /// <summary />
    public class AmazonSqsOptionsTests
    {
        /// <summary />
        [Fact]
        public void DefaultCtor_IsValid()
        {
            var options = new AmazonSqsOptions();

            Assert.Equal(string.Empty, options.ConnectionName);
            Assert.Same(RegionEndpoint.USEast1.SystemName, options.RegionSystemName);
            Assert.Null(options.AccessKey);
            Assert.Null(options.SecretKey);
            Assert.Null(options.SqsConfig);
            Assert.Null(options.SnsConfig);
        }

        /// <summary />
        [Fact]
        public void GetCredentials_GivenNoBasic_ThenReturnsBasicAWSCredentialsWithNull()
        {
            var options = new AmazonSqsOptions();

            var credentials = options.GetCredentials();
            var basic = Assert.IsType<BasicAWSCredentials>(credentials);

            Assert.Null(basic.GetCredentials());
        }

        /// <summary />
        [Fact]
        public void GetCredentials_GivenBasic_ThenReturnsBasicAWSCredentials()
        {
            var options = new AmazonSqsOptions
            {
                AccessKey = "access-key-test",
                SecretKey = "secret-key-test",
            };

            var credentials = options.GetCredentials();
            var basic = Assert.IsType<BasicAWSCredentials>(credentials);

            var immutable = basic.GetCredentials();
            Assert.NotNull(immutable);

            Assert.Equal("access-key-test", immutable.AccessKey);
            Assert.Equal("secret-key-test", immutable.SecretKey);
            Assert.Equal(string.Empty, immutable.Token);
            Assert.False(immutable.UseToken);
        }

        /// <summary />
        [Fact]
        public void GetCredentials_GivenSetCredentials_ThenReturnsSame()
        {
            var options = new AmazonSqsOptions();

            var expected = new AnonymousAWSCredentials();
            options.SetCredentials(expected);

            var credentials = options.GetCredentials();
            var actual = Assert.IsType<AnonymousAWSCredentials>(credentials);

            Assert.Same(expected, actual);
        }

        /// <summary />
        [Fact]
        public void SetCredentials_ThenResetBasic()
        {
            var options = new AmazonSqsOptions
            {
                AccessKey = "access-key-test",
                SecretKey = "secret-key-test",
            };

            Assert.NotNull(options.AccessKey);
            Assert.NotNull(options.SecretKey);

            options.SetCredentials(new AnonymousAWSCredentials());

            Assert.Null(options.AccessKey);
            Assert.Null(options.SecretKey);
        }

    }
}