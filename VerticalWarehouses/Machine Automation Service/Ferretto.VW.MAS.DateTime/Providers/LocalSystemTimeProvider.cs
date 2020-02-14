using System;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.TimeManagement.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

namespace Ferretto.VW.MAS.TimeManagement
{
    internal sealed class LocalSystemTimeProvider : ISystemTimeProvider, IInternalSystemTimeProvider
    {
        #region Fields

        private readonly PubSubEvent<SystemTimeChangedEventArgs> timeChangedEvent;

        #endregion

        #region Constructors

        public LocalSystemTimeProvider(IEventAggregator eventAggregator)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.timeChangedEvent = eventAggregator.GetEvent<PubSubEvent<SystemTimeChangedEventArgs>>();
        }

        #endregion

        #region Properties

        public bool CanEnableWmsAutoSyncMode => false;

        public bool IsWmsAutoSyncEnabled
        {
            get => false;
            set => _ = value;
        }

        #endregion

        #region Methods

        public void SetUtcSystemTime(DateTimeOffset dateTime)
        {
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
