using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    public class CellsConfiguration : IEntityTypeConfiguration<Cell>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<Cell> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(c => c.Id);

            builder
                .HasOne(c => c.LoadingUnit)
                .WithOne(l => l.Cell)
                .HasForeignKey<LoadingUnit>(l => l.CellId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            for (var i = 1; i < 40; i++)
            {
                builder.HasData(
                  new Cell
                  {
                      Id = i,
                      Priority = i,
                      Side = i % 2 == 0? CellSide.Back : CellSide.Front,
                      Status = i < 20 ? CellStatus.Occupied : CellStatus.Free
                  });
            }
        }

        #endregion
    }
}
