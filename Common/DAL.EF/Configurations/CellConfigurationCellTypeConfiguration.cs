using Ferretto.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.DAL.EF.Configurations
{
    public class CellConfigurationCellTypeConfiguration : IEntityTypeConfiguration<CellConfigurationCellType>
    {
        public void Configure(EntityTypeBuilder<CellConfigurationCellType> builder)
        {
            builder.HasKey(c => new { c.CellConfigurationId, c.CellTypeId });

            builder.Property(c => c.Priority)
                .HasDefaultValue(1);

            builder.HasOne(c => c.CellConfiguration)
                .WithMany(c => c.CellConfigurationCellTypes)
                .HasForeignKey(c => c.CellConfigurationId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            builder.HasOne(c => c.CellType)
                .WithMany(c => c.CellConfigurationCellTypes)
                .HasForeignKey(c => c.CellTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
