using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class BayPositionsConfiguration : IEntityTypeConfiguration<BayPosition>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<BayPosition> builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder
                .Property(b => b.Location)
                .HasColumnType("text")
                .HasConversion(
                    enumValue => enumValue.ToString(),
                    stringValue => Enum.Parse<LoadingUnitLocation>(stringValue));

            builder
                .Ignore(b => b.Bay);
        }

        #endregion
    }
}
