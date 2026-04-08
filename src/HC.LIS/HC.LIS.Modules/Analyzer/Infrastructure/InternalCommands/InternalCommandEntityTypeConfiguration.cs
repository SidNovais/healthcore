using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HC.Core.Infrastructure.InternalCommands;

namespace HC.LIS.Modules.Analyzer.Infrastructure.InternalCommands;

internal class InternalCommandEntityTypeConfiguration : IEntityTypeConfiguration<InternalCommand>
  {
    public void Configure(EntityTypeBuilder<InternalCommand> builder)
    {
      builder.ToTable("InternalCommands", "analyzer");

      builder.HasKey(b => b.Id);
      builder.Property(b => b.Id).ValueGeneratedNever();
    }
  }
