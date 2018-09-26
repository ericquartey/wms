using Ferretto.Common.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.EF.Configurations
{
    public class CellTotalConfiguration : IEntityTypeConfiguration<CellTotal>
    {
        public void Configure(EntityTypeBuilder<CellTotal> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(c => c.Id);

            builder.HasOne(c => c.Aisle)
                .WithMany(a => a.CellTotals)
                .HasForeignKey(c => c.AisleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(c => c.CellType)
                .WithMany(c => c.CellTotals)
                .HasForeignKey(c => c.CellTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(c => c.CellStatus)
                .WithMany(c => c.CellTotals)
                .HasForeignKey(c => c.CellStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
