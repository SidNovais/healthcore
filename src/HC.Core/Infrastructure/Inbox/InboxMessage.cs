using System;

namespace HC.Core.Infrastructure.Inbox;

public class InboxMessage
{
    public Guid Id { get; set; }

    public DateTime OccurredAt { get; set; }

    public string? Type { get; set; }

    public string? Data { get; set; }

    public DateTime? ProcessedDate { get; set; }

    public InboxMessage(DateTime occurredAt, string type, string data)
    {
        Id = Guid.CreateVersion7();
        OccurredAt = occurredAt;
        Type = type;
        Data = data;
    }

    private InboxMessage()
    {
    }
}
