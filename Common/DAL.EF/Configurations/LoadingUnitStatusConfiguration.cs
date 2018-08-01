using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class LoadingUnitStatusConfiguration : IEntityTypeConfiguration<LoadingUnitStatus>
  {
    public void Configure(EntityTypeBuilder<LoadingUnitStatus> builder)
    {
      builder.HasKey(l => l.Id);

      builder.Property(l => l.Id).HasColumnType("char(1)");
      builder.Property(l => l.Description).IsRequired();
    }
  }
}
