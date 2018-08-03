using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
  public class CellsGroupConfiguration : IEntityTypeConfiguration<CellsGroup>
  {
    public void Configure(EntityTypeBuilder<CellsGroup> builder)
    {
      if (builder == null)
      {
        throw new System.ArgumentNullException(nameof(builder));
      }

      builder.HasKey(c => c.Id);

      builder.HasOne(c => c.Aisle)
          .WithMany(a => a.CellsGroups)
          .HasForeignKey(c => c.AisleId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(c => c.FirstCell)
          .WithMany(f => f.FirstCellsGroups)
          .HasForeignKey(c => c.FirstCellId)
          .OnDelete(DeleteBehavior.ClientSetNull);
      builder.HasOne(c => c.LastCell)
          .WithMany(l => l.LastCellsGroups)
          .HasForeignKey(c => c.LastCellId)
          .OnDelete(DeleteBehavior.ClientSetNull);
    }
  }
}
