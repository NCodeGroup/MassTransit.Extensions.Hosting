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

using Xunit;

namespace MassTransit.Extensions.Hosting.AzureServiceBusCore.Tests
{
    /// <summary />
    public class ServiceBusOptionsTests
    {
        /// <summary />
        [Fact]
        public void DefaultCtor_IsValid()
        {
            var options = new ServiceBusOptions();

            Assert.Equal(string.Empty, options.ConnectionName);
            Assert.Null(options.HostAddress);
            Assert.Null(options.TokenProvider);
            Assert.Null(options.OperationTimeout);
            Assert.Null(options.RetryMinBackoff);
            Assert.Null(options.RetryMaxBackoff);
            Assert.Null(options.RetryLimit);
            Assert.Null(options.TransportType);
        }

    }
}