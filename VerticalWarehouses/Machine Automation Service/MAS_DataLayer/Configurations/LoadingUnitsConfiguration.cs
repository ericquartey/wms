using System;
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
            var random = new Random();
            for (var i = 1; i <= 15; i++)
            {
                builder.HasData(
                  new LoadingUnit
                  {
                      Id = i,
                      Code = $"LU#1.{i:00}",
                      CellId = i,
                      Status = LoadingUnitStatus.InLocation,
                      Height = 0,
                      Tare = i == 3 || i == 12 || i == 13 ? 65 : 50,
                      GrossWeight = random.Next(200, 400),
                      MissionsCount = random.Next(0, 50),
                      MaxNetWeight = i == 3 || i == 12 || i == 13 ? 750 : 500,
                  });
            }
        }

        #endregion
    }
}
