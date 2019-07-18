using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    public class LoadingUnitsConfiguration : IEntityTypeConfiguration<LoadingUnit>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<LoadingUnit> builder)
        {
            if (builder == null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasKey(l => l.Id);

            for(var i = 1; i < 20; i++)
            {
                builder.HasData(
                  new LoadingUnit
                  {
                      Id = i,
                      Code = $"LU#1.{i:00}",
                      CellId = i,
                      Status = LoadingUnitStatus.InLocation,
                      Height = 100
                  });
            }
        }

        #endregion
    }
}
