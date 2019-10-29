using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class TorqueCurrentMeasurementSessionsConfiguration : IEntityTypeConfiguration<TorqueCurrentMeasurementSession>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<TorqueCurrentMeasurementSession> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder
                .HasKey(e => e.Id);

            builder
                .HasMany(e => e.DataSamples)
                .WithOne(x => x.MeasurementSession)
                .HasForeignKey(s => s.MeasurementSessionId);
        }

        #endregion
    }
}
