using System;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.TimeManagement.Models;
using Microsoft.Extensions.Configuration;
using Prism.Events;

namespace Ferretto.VW.MAS.TimeManagement
{
    internal sealed class SystemTimeProvider : ISystemTimeProvider, IInternalSystemTimeProvider
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly PubSubEvent<SyncStateChangeRequestEventArgs> syncStateChangeRequestEvent;

        private readonly PubSubEvent<SyncStateChangedEventArgs> timeChangedEvent;

        private readonly IWmsSettingsProvider wmsSettingsProvider;

        #endregion

        #region Constructors

        public SystemTimeProvider(
            IWmsSettingsProvider wmsSettingsProvider,
            IConfiguration configuration,
            IEventAggregator eventAggregator)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.wmsSettingsProvider = wmsSettingsProvider ?? throw new ArgumentNullException(nameof(wmsSettingsProvider));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            this.syncStateChangeRequestEvent = eventAggregator.GetEvent<PubSubEvent<SyncStateChangeRequestEventArgs>>();
            this.timeChangedEvent = eventAggregator.GetEvent<PubSubEvent<SyncStateChangedEventArgs>>();
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

                this.syncStateChangeRequestEvent.Publish(new SyncStateChangeRequestEventArgs(value));
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

            this.SetTime(dateTime);
        }

        public void SetTime(DateTime dateTime)
        {
            dateTime.SetAsSystemTime();

            this.timeChangedEvent.Publish(new SyncStateChangedEventArgs(dateTime));
        }

        #endregion
    }
}
