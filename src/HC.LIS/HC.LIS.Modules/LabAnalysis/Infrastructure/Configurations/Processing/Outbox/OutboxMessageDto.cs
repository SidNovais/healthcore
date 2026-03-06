namespace HC.LIS.Modules.LabAnalysis.Infrastructure.Configurations.Processing.Outbox;

public class OutboxMessageDto
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Data { get; set; } = string.Empty;
}
