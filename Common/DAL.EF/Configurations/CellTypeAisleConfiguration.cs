using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
    public class CellTypeAisleConfiguration : IEntityTypeConfiguration<CellTypeAisle>
    {
        public void Configure(EntityTypeBuilder<CellTypeAisle> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.CellTypeTotal)
                .HasDefaultValue(0);
            builder.Property(c => c.Ratio)
                .HasDefaultValue(1);
            builder.Property(c => c.Ratio)
                .HasColumnType("decimal(3, 2)");

            builder.HasOne(c => c.Aisle)
                .WithMany(a => a.AisleCellsTypes)
                .HasForeignKey(c => c.AisleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(c => c.CellType)
                .WithMany(c => c.CellTypeAisles)
                .HasForeignKey(c => c.CellTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
