using System;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class ElevatorAxisManualParametersConfiguration : IEntityTypeConfiguration<ElevatorAxisManualParameters>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<ElevatorAxisManualParameters> builder)
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
