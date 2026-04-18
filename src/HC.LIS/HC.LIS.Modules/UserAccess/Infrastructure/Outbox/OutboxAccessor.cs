using HC.Core.Infrastructure.Outbox;

namespace HC.LIS.Modules.UserAccess.Infrastructure.Outbox;

internal class OutboxAccessor(UserAccessContext context) : IOutbox
{
    private readonly UserAccessContext _context = context;

    public void Add(OutboxMessage message)
    {
        _context.OutboxMessages.Add(message);
    }

    public Task Save()
    {
        // Save is handled automatically by EF Core change tracking during SaveChangesAsync.
        return Task.CompletedTask;
    }
}
