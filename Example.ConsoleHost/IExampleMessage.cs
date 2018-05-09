using System;

namespace Example.ConsoleHost
{
    public interface IExampleMessage
    {
        Guid CorrelationId { get; }

        string StringData { get; }

        DateTime DateTimeData { get; }
    }
}