namespace HC.LIS.Modules.UserAccess.Infrastructure.Configurations.Processing.Inbox;

public class InboxMessageDto
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Data { get; set; } = string.Empty;
}
