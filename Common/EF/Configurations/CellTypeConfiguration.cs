using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class CellTypeConfiguration : IEntityTypeConfiguration<CellType>
    {
        public void Configure(EntityTypeBuilder<CellType> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Description).IsRequired();

            builder.HasOne(c => c.CellHeightClass)
                .WithMany(c => c.CellTypes)
                .HasForeignKey(c => c.CellHeightClassId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(c => c.CellWeightClass)
                .WithMany(c => c.CellTypes)
                .HasForeignKey(c => c.CellWeightClassId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(c => c.CellSizeClass)
                .WithMany(c => c.CellTypes)
                .HasForeignKey(c => c.CellSizeClassId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
