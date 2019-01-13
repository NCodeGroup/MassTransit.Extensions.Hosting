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

namespace MassTransit.Extensions.Hosting.RabbitMq
{
    /// <summary>
    /// Contains the options for configuring a RabbitMQ host.
    /// </summary>
    public class RabbitMqOptions
    {
        private static readonly char[] PathSeparators = { '/' };

        private Uri _hostAddress;
        private string _host;
        private int? _port;
        private string _virtualHost;

        /// <summary>
        /// Gets the default port for RabbitMQ.
        /// </summary>
        public const int DefaultPort = 5672;

        /// <summary>
        /// Gets or sets the client-provided connection name.
        /// </summary>
        public string ConnectionName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the RabbitMQ address to connect to (rabbitmq://host:port/vhost).
        /// </summary>
        public Uri HostAddress
        {
            get => _hostAddress ?? (_hostAddress = FormatHostAddress());
            set
            {
                _hostAddress = value;
                _host = value?.Host;
                _port = value?.IsDefaultPort ?? true ? (int?)null : value.Port;
                _virtualHost = GetVirtualHost(value);
            }
        }

        /// <summary>
        /// Gets or sets the RabbitMQ host to connect to (should be a valid hostname).
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
        /// Gets or sets the RabbitMQ port to connect to.
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
        /// Gets or sets the virtual host for the connection.
        /// </summary>
        public string VirtualHost
        {
            get => _virtualHost;
            set
            {
                _hostAddress = null;
                _virtualHost = value;
            }
        }

        /// <summary>
        /// Gets or sets the heartbeat interval (in seconds) to keep the host connection alive.
        /// </summary>
        public ushort? Heartbeat { get; set; }

        /// <summary>
        /// Gets or sets the username for connecting to the host.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password for connection to the host.
        /// </summary>
        public string Password { get; set; }

        private static string GetVirtualHost(Uri address)
        {
            var path = address?.AbsolutePath;
            if (string.IsNullOrEmpty(path) || path == "/")
                return "/";

            var segments = path.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
            return segments.Length == 0 ? "/" : segments[0];
        }

        private Uri FormatHostAddress()
        {
            var virtualHost = VirtualHost;

            var builder = new UriBuilder
            {
                Scheme = "rabbitmq",
                Host = Host,
                Port = Port ?? 0,
                Path = string.IsNullOrEmpty(virtualHost) || virtualHost == "/"
                    ? "/"
                    : $"/{virtualHost.Trim('/')}"
            };

            return builder.Uri;
        }

    }
}