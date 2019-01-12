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

namespace MassTransit.Extensions.Hosting.Tests
{
    /// <summary />
    public interface IExampleMessage
    {
        /// <summary />
        Guid CorrelationId { get; }

        /// <summary />
        string StringData { get; }

        /// <summary />
        DateTimeOffset DateTimeOffsetData { get; }
    }

    /// <summary />
    public class ExampleMessage : IExampleMessage
    {
        /// <inheritdoc />
        public Guid CorrelationId { get; set; }

        /// <inheritdoc />
        public string StringData { get; set; }

        /// <inheritdoc />
        public DateTimeOffset DateTimeOffsetData { get; set; }
    }
}