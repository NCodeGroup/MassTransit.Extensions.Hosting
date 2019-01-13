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

namespace MassTransit.Extensions.Hosting.RabbitMq.Tests
{
    /// <summary />
    public class RabbitMqOptionsTests
    {
        /// <summary />
        [Fact]
        public void DefaultCtor_IsValid()
        {
            var options = new RabbitMqOptions();

            Assert.Equal(string.Empty, options.ConnectionName);
            Assert.Equal(new Uri("rabbitmq:/"), options.HostAddress);
            Assert.Null(options.Host);
            Assert.Null(options.Port);
            Assert.Null(options.VirtualHost);
            Assert.Null(options.Heartbeat);
            Assert.Null(options.Username);
            Assert.Null(options.Password);
        }

        /// <summary />
        [Fact]
        public void DefaultPort_IsValid()
        {
            Assert.Equal(5672, RabbitMqOptions.DefaultPort);
        }

        /// <summary />
        [Fact]
        public void AssignHostAddress_OtherPort_PropertiesAreValid()
        {
            var options = new RabbitMqOptions
            {
                HostAddress = new Uri("rabbitmq://localhost:8080/vhost")
            };

            Assert.Equal("localhost", options.Host);
            Assert.Equal("localhost:8080", options.HostAddress.Authority);
            Assert.Equal(8080, options.Port);
            Assert.Equal("vhost", options.VirtualHost);
        }

        /// <summary />
        [Fact]
        public void AssignHostAddress_EmptyPort_PropertiesAreValid()
        {
            var options = new RabbitMqOptions
            {
                HostAddress = new Uri("rabbitmq://localhost/vhost")
            };

            Assert.Equal("localhost", options.Host);
            Assert.Equal("localhost", options.HostAddress.Authority);
            Assert.Null(options.Port);
            Assert.Equal("vhost", options.VirtualHost);
        }

        /// <summary />
        [Fact]
        public void AssignHostAddress_DefaultPort_PropertiesAreValid()
        {
            var options = new RabbitMqOptions
            {
                HostAddress = new Uri("rabbitmq://localhost:5672/vhost")
            };

            Assert.Equal("localhost", options.Host);
            Assert.Equal("localhost:5672", options.HostAddress.Authority);
            Assert.Equal(5672, options.Port);
            Assert.Equal("vhost", options.VirtualHost);
        }

        /// <summary />
        [Fact]
        public void AssignProperties_HostAddressIsValid()
        {
            var options = new RabbitMqOptions
            {
                Host = "localhost",
                Port = 8080,
                VirtualHost = "vhost",
            };

            Assert.Equal("localhost", options.HostAddress.Host);
            Assert.Equal("localhost:8080", options.HostAddress.Authority);
            Assert.Equal(8080, options.HostAddress.Port);
            Assert.Equal("/vhost", options.HostAddress.AbsolutePath);
        }

        /// <summary />
        [Fact]
        public void AssignNullVirtualHost_HostAddressIsValid()
        {
            var options = new RabbitMqOptions
            {
                Host = "localhost",
                VirtualHost = null,
            };

            Assert.Equal("/", options.HostAddress.AbsolutePath);
        }

        /// <summary />
        [Fact]
        public void AssignEmptyVirtualHost_HostAddressIsValid()
        {
            var options = new RabbitMqOptions
            {
                Host = "localhost",
                VirtualHost = string.Empty,
            };

            Assert.Equal("/", options.HostAddress.AbsolutePath);
        }

        /// <summary />
        [Fact]
        public void AssignVirtualHostWithMoreParts_HostAddressIsValid()
        {
            var options = new RabbitMqOptions
            {
                Host = "localhost",
                VirtualHost = "/part1/part2",
            };

            // even though this is NOT a valid RabbitMQ host address,
            // we still validate the URI parsing...
            Assert.Equal("/part1/part2", options.HostAddress.AbsolutePath);
        }

        /// <summary />
        [Fact]
        public void AssignNullPort_HostAddressIsValid()
        {
            var options = new RabbitMqOptions
            {
                Host = "localhost",
                Port = null,
            };

            Assert.Equal("localhost", options.HostAddress.Authority);
            Assert.Equal(-1, options.HostAddress.Port);
        }

        /// <summary />
        [Fact]
        public void AssignZeroPort_HostAddressIsValid()
        {
            var options = new RabbitMqOptions
            {
                Host = "localhost",
                Port = 0,
            };

            Assert.Equal("localhost", options.HostAddress.Authority);
            Assert.Equal(-1, options.HostAddress.Port);
        }

        /// <summary />
        [Fact]
        public void AssignDefaultPort_HostAddressIsValid()
        {
            var options = new RabbitMqOptions
            {
                Host = "localhost",
                Port = 5672,
            };

            Assert.Equal("localhost:5672", options.HostAddress.Authority);
            Assert.Equal(5672, options.HostAddress.Port);
        }

        /// <summary />
        [Fact]
        public void AssignOtherPort_HostAddressIsValid()
        {
            var options = new RabbitMqOptions
            {
                Host = "localhost",
                Port = 8080,
            };

            Assert.Equal("localhost:8080", options.HostAddress.Authority);
            Assert.Equal(8080, options.HostAddress.Port);
        }

    }
}