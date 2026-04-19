using HC.Core.Infrastructure.InternalCommands;
using HC.Core.Infrastructure.Outbox;
using HC.LIS.Modules.UserAccess.Domain.Users;
using HC.LIS.Modules.UserAccess.Infrastructure.InternalCommands;
using HC.LIS.Modules.UserAccess.Infrastructure.Outbox;
using HC.LIS.Modules.UserAccess.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;

namespace HC.LIS.Modules.UserAccess.Infrastructure;

public class UserAccessContext(
    DbContextOptions options
) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<InternalCommand> InternalCommands { get; set; }
    internal DbSet<OutboxMessage> OutboxMessages { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new InternalCommandEntityTypeConfiguration());
    }
}
