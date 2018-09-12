using Ferretto.Common.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.Common.Modules.DAL.EF.Configurations
{
    public class CellConfigurationCellTypeConfiguration : IEntityTypeConfiguration<CellConfigurationCellType>
    {
        public void Configure(EntityTypeBuilder<CellConfigurationCellType> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(c => new {c.CellConfigurationId, c.CellTypeId});

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
