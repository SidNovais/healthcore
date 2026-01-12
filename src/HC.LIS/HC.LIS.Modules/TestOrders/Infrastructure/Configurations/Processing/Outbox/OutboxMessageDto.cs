namespace HC.LIS.Modules.TestOrders.Infrastructure.Configurations.Processing.Outbox;

public class OutboxMessageDto
{
    public Guid Id { get; set; }

    public string Type { get; set; }

    public string Data { get; set; }
}
