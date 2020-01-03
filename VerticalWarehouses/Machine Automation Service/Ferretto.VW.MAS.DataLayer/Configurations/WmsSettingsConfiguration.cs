using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class WmsSettingsConfiguration : IEntityTypeConfiguration<WmsSettings>
    {
        #region Fields

        private const int DefaultTimeSyncInterval = 10 * 1000;

        #endregion

        #region Methods

        public void Configure(EntityTypeBuilder<WmsSettings> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.HasData(new WmsSettings { Id = -1, IsWmsTimeSyncEnabled = true, TimeSyncIntervalMilliseconds = DefaultTimeSyncInterval });
        }

        #endregion
    }
}
