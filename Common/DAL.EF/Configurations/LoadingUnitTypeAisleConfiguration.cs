using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class LoadingUnitTypeAisleConfiguration : IEntityTypeConfiguration<LoadingUnitTypeAisle>
  {
    public void Configure(EntityTypeBuilder<LoadingUnitTypeAisle> builder)
    {
      builder.HasKey(l => new { l.AisleId, l.LoadingUnitTypeId });

      builder.HasOne(l => l.Aisle)
          .WithMany(a => a.AisleLoadingUnitTypes)
          .HasForeignKey(l => l.AisleId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(l => l.LoadingUnit)
          .WithMany(l => l.LoadingUnitTypeAisles)
          .HasForeignKey(l => l.LoadingUnitTypeId)
          .OnDelete(DeleteBehavior.ClientSetNull);
    }
  }
}
