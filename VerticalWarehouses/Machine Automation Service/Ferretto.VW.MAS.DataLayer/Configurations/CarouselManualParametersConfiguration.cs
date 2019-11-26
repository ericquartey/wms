using System;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class CarouselManualParametersConfiguration : IEntityTypeConfiguration<CarouselManualParameters>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<CarouselManualParameters> builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.HasKey(l => l.Id);
        }

        #endregion
    }
}
