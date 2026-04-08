using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HC.Core.Infrastructure.Outbox;

namespace HC.LIS.Modules.Analyzer.Infrastructure.Outbox;

  internal class OutboxMessageEntityTypeConfiguration : IEntityTypeConfiguration<OutboxMessage>
  {
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
      builder.ToTable("OutboxMessages", "analyzer");

      builder.HasKey(b => b.Id);
      builder.Property(b => b.Id).ValueGeneratedNever();
    }
  }
