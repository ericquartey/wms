using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
    public class CellConfiguration : IEntityTypeConfiguration<Cell>
    {
        public void Configure(EntityTypeBuilder<Cell> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Floor).IsRequired();
            builder.Property(c => c.Column).IsRequired();
            builder.Property(c => c.Class).IsRequired()
                .HasColumnType("char(1)");
            builder.Property(c => c.Side).IsRequired()
                .HasColumnType("char(1)");
            builder.Property(c => c.Priority)
                .HasDefaultValue(1);

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
