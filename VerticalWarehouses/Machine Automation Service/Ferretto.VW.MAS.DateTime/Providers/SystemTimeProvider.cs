using System;
using Ferretto.VW.MAS.DataLayer;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS.TimeManagement
{
    public sealed class SystemTimeProvider : ISystemTimeProvider
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly ISystemTimeSyncService systemTimeSyncService;

        private readonly IWmsSettingsProvider wmsSettingsProvider;

        #endregion

        #region Constructors

        public SystemTimeProvider(
            IWmsSettingsProvider wmsSettingsProvider,
            IConfiguration configuration,
            ISystemTimeSyncService systemTimeService)
        {
            this.wmsSettingsProvider = wmsSettingsProvider ?? throw new ArgumentNullException(nameof(wmsSettingsProvider));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.systemTimeSyncService = systemTimeService ?? throw new ArgumentNullException(nameof(configuration));
        }

        #endregion

        #region Properties

        public bool CanEnableWmsAutoSyncMode => this.configuration.IsWmsEnabled();

        public bool IsWmsAutoSyncEnabled
        {
            get => this.wmsSettingsProvider.IsWmsTimeSyncEnabled;
            set
            {
                if (!this.CanEnableWmsAutoSyncMode && value)
                {
                    throw new InvalidOperationException("Unable to enable WMS auto sync because WMS is not enabled.");
                }

                this.wmsSettingsProvider.IsWmsTimeSyncEnabled = value;

                if (value)
                {
                    this.systemTimeSyncService.Enable();
                }
                else
                {
                    this.systemTimeSyncService.Disable();
                }
            }
        }

        #endregion

        #region Methods

        public void SetSystemTime(DateTime dateTime)
        {
            if (this.IsWmsAutoSyncEnabled)
            {
                throw new InvalidOperationException("Cannot manually set system time when WMS auto sync is enabled.");
            }

            this.systemTimeSyncService.SetSystemTime(dateTime);
        }

        #endregion
    }
}
