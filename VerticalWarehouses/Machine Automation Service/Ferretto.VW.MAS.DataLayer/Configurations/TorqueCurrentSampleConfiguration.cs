using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class TorqueCurrentSampleConfiguration : IEntityTypeConfiguration<TorqueCurrentSample>
    {
        #region Methods

        public void Configure(EntityTypeBuilder<TorqueCurrentSample> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder
                .HasKey(s => s.Id);
        }

        #endregion
    }
}
