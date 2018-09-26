using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class CellConfiguration : IEntityTypeConfiguration<Cell>
    {
        public void Configure(EntityTypeBuilder<Cell> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Floor).IsRequired();
            builder.Property(c => c.Column).IsRequired();
            builder.Property(c => c.ClassId).IsRequired()
                .HasColumnType("char(1)");
            builder.Property(c => c.Side).IsRequired()
                .HasColumnType("char(1)");
            builder.Property(c => c.Priority)
                .HasDefaultValue(1);

            builder.HasOne(c => c.AbcClass)
                .WithMany(a => a.Cells)
                .HasForeignKey(c => c.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(c => c.Aisle)
                .WithMany(a => a.Cells)
                .HasForeignKey(c => c.AisleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(c => c.CellType)
                .WithMany(c => c.Cells)
                .HasForeignKey(c => c.CellTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(c => c.CellStatus)
                .WithMany(c => c.Cells)
                .HasForeignKey(c => c.CellStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
