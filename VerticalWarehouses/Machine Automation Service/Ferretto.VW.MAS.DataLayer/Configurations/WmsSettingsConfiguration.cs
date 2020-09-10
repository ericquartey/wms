using System;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ferretto.VW.MAS.DataLayer.Configurations
{
    internal class WmsSettingsConfiguration : IEntityTypeConfiguration<WmsSettings>
    {
        #region Fields

        private const int DefaultTimeSyncInterval = 60 * 1000;

        #endregion

        #region Methods

        public void Configure(EntityTypeBuilder<WmsSettings> builder)
        {
            if (builder is null)
            {
                throw new System.ArgumentNullException(nameof(builder));
            }

            builder.Property(s => s.ServiceUrl)
             .HasColumnType("text")
             .HasConversion(
                 uri => uri.ToString(),
                 stringValue => new Uri(stringValue));

            builder.HasData(
                new WmsSettings
                {
                    Id = -1,
                    IsEnabled = false,
                    IsTimeSyncEnabled = false,
                    TimeSyncIntervalMilliseconds = DefaultTimeSyncInterval,
                    ServiceUrl = new Uri("http://127.0.0.1:10000"),
                    SocketLinkIsEnabled = false,
                    SocketLinkPort = 7075,
                    SocketLinkTimeout = 600,
                    SocketLinkPolling = 120
                });
        }

        #endregion
    }
}
