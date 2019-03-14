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

namespace MassTransit.Extensions.Hosting.Http
{
    /// <summary>
    /// Contains the options for configuring a HTTP host.
    /// </summary>
    public class HttpOptions
    {
        /// <summary>
        /// Gets or sets the client-provided connection name.
        /// </summary>
        public string ConnectionName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the HTTP scheme (i.e. HTTP or HTTPS) to use.
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// Gets or sets the HTTP host to connect to.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the HTTP port to connect to.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the HTTP method (i.e. verb) to use.
        /// </summary>
        public string Method { get; set; }
    }
}