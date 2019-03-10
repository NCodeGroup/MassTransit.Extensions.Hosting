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

namespace MassTransit.Extensions.Hosting.ActiveMq
{
    /// <summary>
    /// Contains the options for configuring a ActiveMQ host.
    /// </summary>
    public class ActiveMqOptions
    {
        private Uri _hostAddress;
        private string _host;
        private int? _port;
        private bool _useSsl;

        /// <summary>
        /// Gets the default port for ActiveMQ without SSL.
        /// </summary>
        public const int DefaultPort = 61616;

        /// <summary>
        /// Gets the default port for ActiveMQ with SSL.
        /// </summary>
        public const int DefaultPortSsl = 61617;

        /// <summary>
        /// Gets or sets the client-provided connection name.
        /// </summary>
        public string ConnectionName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ActiveMQ address to connect to (rabbitmq://host:port/vhost).
        /// </summary>
        public Uri HostAddress
        {
            get => _hostAddress ?? (_hostAddress = FormatHostAddress(Host, Port, UseSsl));
            set
            {
                _hostAddress = value;
                _host = value?.Host;
                _port = value?.IsDefaultPort ?? true ? (int?)null : value.Port;
            }
        }

        /// <summary>
        /// Gets or sets the ActiveMQ host to connect to (should be a valid hostname).
        /// </summary>
        public string Host
        {
            get => _host;
            set
            {
                _hostAddress = null;
                _host = value;
            }
        }

        /// <summary>
        /// Gets or sets the ActiveMQ port to connect to.
        /// </summary>
        public int? Port
        {
            get => _port;
            set
            {
                _hostAddress = null;
                _port = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the ActiveMQ connection should use SSL or not.
        /// </summary>
        public bool UseSsl
        {
            get => _useSsl;
            set
            {
                _hostAddress = null;
                _useSsl = value;
            }
        }

        /// <summary>
        /// Gets or sets the username for connecting to the host.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password for connection to the host.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Returns the URI address for connecting to an ActiveMQ host.
        /// </summary>
        /// <param name="host">Contains the ActiveMQ host to connect to.</param>
        /// <param name="port">Contains the ActiveMQ port to connect to.</param>
        /// <param name="useSsl">Indicates whether the ActiveMQ connection should use SSL or not.</param>
        public static Uri FormatHostAddress(string host, int? port, bool useSsl)
        {
            var builder = new UriBuilder
            {
                Scheme = "activemq",
                Host = host,
                Port = port ?? (useSsl ? DefaultPortSsl : DefaultPort),
                Path = "/",
            };

            return builder.Uri;
        }

    }
}