using Ferretto.VW.MAS.DataModels.Cells;
using Ferretto.VW.MAS.DataModels.LoadingUnits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class CellsConfiguration : IEntityTypeConfiguration<Cell>
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

            for (var i = 1; i <= 378; i++)
            {
                var status = CellStatus.Free;
                if (i < 40)
                {
                    status = CellStatus.Occupied;
                }
                else if (i < 50)
                {
                    status = CellStatus.Unusable;
                }
                else if (i < 60)
                {
                    status = CellStatus.Disabled;
                }

                builder.HasData(
                  new Cell
                  {
                      Id = i,
                      Priority = i,
                      Side = i % 2 == 0 ? CellSide.Back : CellSide.Front,
                      Status = status,
                      Coord = 268 + (i - i % 2) * 25
                  });
            }
        }

        #endregion
    }
}
