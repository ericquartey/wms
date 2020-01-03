using System;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class LoadingUnitsConfiguration : IEntityTypeConfiguration<LoadingUnit>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<LoadingUnit> builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Status)
                .HasColumnType("text")
                .HasConversion(
                    enumValue => enumValue.ToString(),
                    stringValue => Enum.Parse<LoadingUnitStatus>(stringValue));

            builder
                .Ignore(l => l.Code)
                .Ignore(l => l.NetWeight);
        }

        #endregion
    }
}
