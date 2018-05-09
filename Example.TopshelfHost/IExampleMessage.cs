using System;

namespace Example.TopshelfHost
{
    public interface IExampleMessage
    {
        Guid CorrelationId { get; }

        string StringData { get; }

        DateTime DateTimeData { get; }
    }
}