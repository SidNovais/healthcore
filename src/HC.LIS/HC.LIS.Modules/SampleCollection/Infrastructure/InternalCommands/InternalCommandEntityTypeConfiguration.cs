using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using HC.Core.Infrastructure.InternalCommands;

namespace HC.LIS.Modules.SampleCollection.Infrastructure.InternalCommands;

internal class InternalCommandEntityTypeConfiguration : IEntityTypeConfiguration<InternalCommand>
  {
    public void Configure(EntityTypeBuilder<InternalCommand> builder)
    {
      builder.ToTable("InternalCommands", "sample_collection");

      builder.HasKey(b => b.Id);
      builder.Property(b => b.Id).ValueGeneratedNever();
    }
  }
