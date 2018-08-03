using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
{
  public class LoadingUnitTypeAisleConfiguration : IEntityTypeConfiguration<LoadingUnitTypeAisle>
  {
    public void Configure(EntityTypeBuilder<LoadingUnitTypeAisle> builder)
    {
      if (builder == null)
      {
        throw new System.ArgumentNullException(nameof(builder));
      }

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
