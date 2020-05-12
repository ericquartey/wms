using System;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.TimeManagement.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

namespace Ferretto.VW.MAS.TimeManagement
{
    internal sealed class WmsSystemTimeProvider : ISystemTimeProvider, IInternalSystemTimeProvider
    {
        #region Fields

        private readonly IConfiguration configuration;

        private readonly IServiceScopeFactory serviceScopeFactory;

        private readonly PubSubEvent<SyncStateChangeRequestEventArgs> syncStateChangeRequestEvent;

        private readonly PubSubEvent<SystemTimeChangedEventArgs> timeChangedEvent;

        #endregion

        #region Constructors

        public WmsSystemTimeProvider(
            IConfiguration configuration,
            IEventAggregator eventAggregator,
            IServiceScopeFactory serviceScopeFactory)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            this.syncStateChangeRequestEvent = eventAggregator.GetEvent<PubSubEvent<SyncStateChangeRequestEventArgs>>();
            this.timeChangedEvent = eventAggregator.GetEvent<PubSubEvent<SystemTimeChangedEventArgs>>();
        }

        #endregion

        #region Properties

        public bool CanEnableWmsAutoSyncMode => this.serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IWmsSettingsProvider>().IsEnabled;

        public bool IsWmsAutoSyncEnabled
        {
            get => this.serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IWmsSettingsProvider>().IsTimeSyncEnabled;
            set
            {
                if (!this.CanEnableWmsAutoSyncMode && value)
                {
                    throw new InvalidOperationException("Unable to enable WMS auto sync because WMS is not enabled.");
                }

                this.serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IWmsSettingsProvider>().IsTimeSyncEnabled = value;

                this.syncStateChangeRequestEvent.Publish(new SyncStateChangeRequestEventArgs(value));
            }
        }

        #endregion

        #region Methods

        public void SetUtcSystemTime(DateTimeOffset dateTime)
        {
            if (this.IsWmsAutoSyncEnabled)
            {
                throw new InvalidOperationException("Cannot manually set system time when WMS auto sync is enabled.");
            }

            this.SetUtcTime(dateTime);
        }

        public void SetUtcTime(DateTimeOffset dateTime)
        {
            dateTime.SetAsUtcSystemTime();

            this.timeChangedEvent.Publish(new SystemTimeChangedEventArgs(dateTime));
        }

        #endregion
    }
}
