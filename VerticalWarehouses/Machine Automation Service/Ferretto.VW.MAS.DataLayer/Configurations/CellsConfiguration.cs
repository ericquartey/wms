using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class CellsConfiguration : IEntityTypeConfiguration<Cell>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Cell> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(c => c.Id);

            builder
                .HasOne(c => c.LoadingUnit)
                .WithOne(l => l.Cell)
                .HasForeignKey<LoadingUnit>(l => l.CellId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder
               .HasOne(c => c.Elevator)
               .WithOne(e => e.Cell)
               .HasForeignKey<Elevator>(e => e.CellId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Property(c => c.Status)
                .HasColumnType("text")
                .HasConversion(
                    enumValue => enumValue.ToString(),
                    stringValue => System.Enum.Parse<CellStatus>(stringValue));

            builder
                .HasOne(c => c.Panel)
                .WithMany(p => p.Cells)
                .HasForeignKey(c => c.PanelId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        #endregion
    }
}
