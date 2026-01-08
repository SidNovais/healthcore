using HC.Core.Infrastructure.InternalCommands;
using HC.Core.Infrastructure.Outbox;
using HC.LIS.Modules.TestOrders.Infrastructure.InternalCommands;
using HC.LIS.Modules.TestOrders.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace HC.LIS.Modules.TestOrders.Infrastructure;

public class TestOrdersContext(
    DbContextOptions options
) : DbContext(options)
{
    public DbSet<InternalCommand> InternalCommands { get; set; }
    internal DbSet<OutboxMessage> OutboxMessages { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new InternalCommandEntityTypeConfiguration());
    }
}
