using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_Utils.Events;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer
{
    internal class BaysProvider : IBaysProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly NotificationEvent notificationEvent;

        #endregion

        #region Constructors

        public BaysProvider(DataLayerContext dataContext, IEventAggregator eventAggregator)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (dataContext == null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            this.dataContext = dataContext;
            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
        }

        #endregion

        #region Methods

        public Bay Activate(int id)
        {
            var bay = this.GetById(id);
            if (bay != null)
            {
                bay.IsActive = true;

                this.Update(bay);

                this.NotifyBayStatusChanged(bay);
            }

            return bay;
        }

        public void Create(Bay bay)
        {
            this.dataContext.Bays.Add(bay);

            this.dataContext.SaveChanges();
        }

        public Bay Deactivate(int id)
        {
            var bay = this.GetById(id);
            if (bay != null)
            {
                bay.IsActive = false;

                this.Update(bay);
            }

            return bay;
        }

        public IEnumerable<Bay> GetAll()
        {
            return this.dataContext.Bays.ToArray();
        }

        public Bay GetById(int id)
        {
            return this.dataContext.Bays.SingleOrDefault(b => b.Id == id);
        }

        public Bay GetByIpAddress(IPAddress remoteIpAddress)
        {
            return this.dataContext.Bays.SingleOrDefault(b => b.IpAddress == remoteIpAddress.ToString());
        }

        public void Update(int id, string ipAddress, BayType bayType)
        {
            var bay = this.GetById(id);
            if (bay != null)
            {
                bay.IpAddress = ipAddress;
                bay.Type = bayType;

                this.Update(bay);
            }
        }

        private void NotifyBayStatusChanged(Bay bay)
        {
            var message = new NotificationMessage(
             new BayOperationalStatusChangedMessageData
             {
                 BayId = bay.Id,
                 BayStatus = bay.Status,
             },
             $"Bay #{bay.Id} status changed to {bay.Status}",
             MessageActor.MissionsManager,
             MessageActor.WebApi,
             MessageType.BayOperationalStatusChanged,
             MessageStatus.NoStatus);

            this.notificationEvent.Publish(message);
        }

        private Bay Update(Bay bay)
        {
            var entry = this.dataContext.Bays.Update(bay);

            this.dataContext.SaveChanges();

            this.NotifyBayStatusChanged(bay);

            return entry.Entity;
        }

        #endregion
    }
}
