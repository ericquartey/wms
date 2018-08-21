using Ferretto.Common.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
{
  public class CellConfigurationCellPositionLoadingUnitTypeConfiguration : IEntityTypeConfiguration<CellConfigurationCellPositionLoadingUnitType>
  {
    public void Configure(EntityTypeBuilder<CellConfigurationCellPositionLoadingUnitType> builder)
    {
      if (builder == null)
      {
        throw new System.ArgumentNullException(nameof(builder));
      }

      builder.HasKey(c => new { c.CellPositionId, c.CellConfigurationId, c.LoadingUnitTypeId });

      builder.Property(c => c.Priority)
          .HasDefaultValue(1);

      builder.HasOne(c => c.CellPosition)
          .WithMany(c => c.CellConfigurationCellPositionLoadingUnitTypes)
          .HasForeignKey(c => c.CellPositionId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(c => c.CellConfiguration)
          .WithMany(c => c.CellConfigurationCellPositionLoadingUnitTypes)
          .HasForeignKey(c => c.CellConfigurationId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(c => c.LoadingUnitType)
          .WithMany(l => l.CellConfigurationCellPositionLoadingUnitTypes)
          .HasForeignKey(c => c.LoadingUnitTypeId)
          .OnDelete(DeleteBehavior.ClientSetNull);
    }
  }
}
