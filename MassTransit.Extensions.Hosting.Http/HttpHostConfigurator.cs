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
using System.Net.Http;
using MassTransit.HttpTransport;
using MassTransit.HttpTransport.Hosting;

namespace MassTransit.Extensions.Hosting.Http
{
    internal class HttpHostConfigurator : IHttpHostConfigurator, HttpHostSettings
    {
        public HttpHostConfigurator(string scheme, string host, int port)
        {
            Scheme = scheme ?? throw new ArgumentNullException(nameof(scheme));
            Host = host ?? throw new ArgumentNullException(nameof(host));
            Port = port;
        }

        public HttpHostSettings Settings => this;

        public string Scheme { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public HttpMethod Method { get; set; }

        public string Description => $"{Host}:{Port}";
    }
}