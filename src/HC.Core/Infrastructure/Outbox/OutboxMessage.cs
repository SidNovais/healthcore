using System;

namespace HC.Core.Infrastructure.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public string? Type { get; set; }

    public string? Data { get; set; }

    public DateTime? ProcessedDate { get; set; }

    public string? TraceContext { get; set; }

    public OutboxMessage(Guid id, DateTime occurredAt, string type, string data, string? traceContext = null)
    {
        Id = id;
        OccurredAt = occurredAt;
        Type = type;
        Data = data;
        TraceContext = traceContext;
    }

    private OutboxMessage()
    {
    }
}
