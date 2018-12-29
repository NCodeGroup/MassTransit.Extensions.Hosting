#region Copyright Preamble
// 
//    Copyright @ 2018 NCode Group
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
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using Xunit.Abstractions;

namespace MassTransit.Extensions.Hosting.Tests
{
    /// <summary />
    public class ExampleConsumer : IConsumer<IExampleMessage>
    {
        private readonly ITestOutputHelper _output;

        /// <summary />
        public ExampleConsumer(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        /// <summary />
        public Task Consume(ConsumeContext<IExampleMessage> context)
        {
            var json = JsonConvert.SerializeObject(context.Message, Formatting.Indented);
            _output.WriteLine($"ExampleConsumer: {json}");

            var hasServiceProvider = context.TryGetPayload<IServiceProvider>(out var serviceProvider);
            Assert.True(hasServiceProvider && serviceProvider != null);

            var hasServiceScope = context.TryGetPayload<IServiceScope>(out var serviceScope);
            Assert.True(hasServiceScope && serviceScope != null);

            Assert.AreSame(serviceProvider, serviceScope.ServiceProvider);

            return Task.CompletedTask;
        }

    }
}