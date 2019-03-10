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
using Xunit;

namespace MassTransit.Extensions.Hosting.ActiveMq.Tests
{
    /// <summary />
    public class ActiveMqOptionsTests
    {
        /// <summary />
        [Fact]
        public void DefaultCtor_IsValid()
        {
            var options = new ActiveMqOptions();

            Assert.Equal(string.Empty, options.ConnectionName);
            Assert.Equal(new Uri("activemq:/"), options.HostAddress);
            Assert.Null(options.Host);
            Assert.Null(options.Port);
            Assert.Null(options.Username);
            Assert.Null(options.Password);
        }

        /// <summary />
        [Fact]
        public void DefaultPort_IsValid()
        {
            Assert.Equal(61616, ActiveMqOptions.DefaultPort);
            Assert.Equal(61617, ActiveMqOptions.DefaultPortSsl);
        }

        /// <summary />
        [Fact]
        public void AssignHostAddress_OtherPort_PropertiesAreValid()
        {
            var options = new ActiveMqOptions
            {
                HostAddress = new Uri("activemq://localhost:8080/")
            };

            Assert.Equal("localhost", options.Host);
            Assert.Equal("localhost", options.HostAddress.Host);
            Assert.Equal(8080, options.Port);
        }

        /// <summary />
        [Fact]
        public void AssignHostAddress_EmptyPort_PropertiesAreValid()
        {
            var options = new ActiveMqOptions
            {
                HostAddress = new Uri("activemq://localhost")
            };

            Assert.Equal("localhost", options.Host);
            Assert.Equal("localhost", options.HostAddress.Host);
            Assert.Null(options.Port);
        }

        /// <summary />
        [Fact]
        public void AssignHostAddress_DefaultPort_PropertiesAreValid()
        {
            var options = new ActiveMqOptions
            {
                HostAddress = new Uri("activemq://localhost:61616")
            };

            Assert.Equal("localhost", options.Host);
            Assert.Equal(61616, options.Port);
        }

        /// <summary />
        [Fact]
        public void AssignProperties_HostAddressIsValid()
        {
            var options = new ActiveMqOptions
            {
                Host = "localhost",
                Port = 8080,
            };

            Assert.Equal("localhost", options.HostAddress.Host);
            Assert.Equal(8080, options.HostAddress.Port);
        }

        /// <summary />
        [Fact]
        public void AssignNullPort_WithoutSsl_HostAddressIsValid()
        {
            var options = new ActiveMqOptions
            {
                Host = "localhost",
                Port = null,
                UseSsl = false,
            };

            Assert.Equal("localhost", options.HostAddress.Host);
            Assert.Equal(61616, options.HostAddress.Port);
        }

        /// <summary />
        [Fact]
        public void AssignNullPort_WithSsl_HostAddressIsValid()
        {
            var options = new ActiveMqOptions
            {
                Host = "localhost",
                Port = null,
                UseSsl = true,
            };

            Assert.Equal("localhost", options.HostAddress.Host);
            Assert.Equal(61617, options.HostAddress.Port);
        }

        /// <summary />
        [Fact]
        public void AssignZeroPort_HostAddressIsValid()
        {
            var options = new ActiveMqOptions
            {
                Host = "localhost",
                Port = 0,
            };

            Assert.Equal("localhost", options.HostAddress.Host);
            Assert.Equal(-1, options.HostAddress.Port);
        }

        /// <summary />
        [Fact]
        public void AssignDefaultPort_WithoutSsl_HostAddressIsValid()
        {
            var options = new ActiveMqOptions
            {
                Host = "localhost",
                Port = 61616,
            };

            Assert.Equal("localhost", options.HostAddress.Host);
            Assert.Equal(61616, options.HostAddress.Port);
        }

        /// <summary />
        [Fact]
        public void AssignOtherPort_WithoutSsl_HostAddressIsValid()
        {
            var options = new ActiveMqOptions
            {
                Host = "localhost",
                Port = 8080,
                UseSsl = false,
            };

            Assert.Equal("localhost", options.HostAddress.Host);
            Assert.Equal(8080, options.HostAddress.Port);
        }

        /// <summary />
        [Fact]
        public void AssignOtherPort_WithSsl_HostAddressIsValid()
        {
            var options = new ActiveMqOptions
            {
                Host = "localhost",
                Port = 8080,
                UseSsl = true,
            };

            Assert.Equal("localhost", options.HostAddress.Host);
            Assert.Equal(8080, options.HostAddress.Port);
        }

    }
}