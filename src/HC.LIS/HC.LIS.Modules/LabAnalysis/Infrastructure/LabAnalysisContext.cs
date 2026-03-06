using HC.Core.Infrastructure.InternalCommands;
using HC.Core.Infrastructure.Outbox;
using HC.LIS.Modules.LabAnalysis.Infrastructure.InternalCommands;
using HC.LIS.Modules.LabAnalysis.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace HC.LIS.Modules.LabAnalysis.Infrastructure;

public class LabAnalysisContext(
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
