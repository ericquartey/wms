using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Exceptions;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class BaysProvider : Interfaces.IBaysProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly NotificationEvent notificationEvent;

        #endregion

        #region Constructors

        public BaysProvider(DataLayerContext dataContext, IEventAggregator eventAggregator)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (dataContext is null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            this.dataContext = dataContext;
            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
        }

        #endregion

        #region Methods

        public Bay Activate(int bayNumber)
        {
            var bay = this.GetByNumber(bayNumber);
            if (bay is null)
            {
                throw new EntityNotFoundException(bayNumber);
            }

            bay.IsActive = true;

            this.Update(bay);

            return bay;
        }

        public Bay AssignMissionOperation(int bayNumber, int? missionId, int? missionOperationId)
        {
            var bay = this.GetByNumber(bayNumber);
            if (bay is null)
            {
                throw new EntityNotFoundException(bayNumber);
            }

            bay.CurrentMissionId = missionId;
            bay.CurrentMissionOperationId = missionOperationId;

            this.Update(bay);

            return bay;
        }

        public void Create(Bay bay)
        {
            if (bay is null)
            {
                throw new ArgumentNullException(nameof(bay));
            }

            this.dataContext.Bays.Add(bay);

            this.dataContext.SaveChanges();
        }

        public Bay Deactivate(int bayNumber)
        {
            var bay = this.GetByNumber(bayNumber);
            if (bay is null)
            {
                throw new EntityNotFoundException(bayNumber);
            }

            bay.IsActive = false;

            this.Update(bay);

            return bay;
        }

        public IEnumerable<Bay> GetAll()
        {
            return this.dataContext.Bays.ToArray();
        }

        public Bay GetByIpAddress(IPAddress remoteIpAddress)
        {
            return this.dataContext.Bays
                .SingleOrDefault(b => b.IpAddress == remoteIpAddress.ToString());
        }

        public Bay GetByNumber(int bayNumber)
        {
            var bay = this.dataContext.Bays.SingleOrDefault(b => b.Number == bayNumber);
            if (bay is null)
            {
                throw new EntityNotFoundException(bayNumber);
            }

            return bay;
        }

        public void Update(int bayNumber, string ipAddress, BayType bayType)
        {
            var bay = this.GetByNumber(bayNumber);
            if (bay is null)
            {
                throw new EntityNotFoundException(bayNumber);
            }
            bay.IpAddress = ipAddress;
            bay.Type = bayType;

            this.Update(bay);
        }

        private Bay Update(Bay bay)
        {
            if (bay is null)
            {
                throw new ArgumentNullException(nameof(bay));
            }

            var entry = this.dataContext.Bays.Update(bay);

            this.dataContext.SaveChanges();

            this.notificationEvent.Publish(
                new NotificationMessage(
                    new BayOperationalStatusChangedMessageData
                    {
                        BayNumber = bay.Number,
                        BayStatus = bay.Status,
                    },
                    $"Bay #{bay.Number} status changed to {bay.Status}",
                    MessageActor.MissionsManager,
                    MessageActor.WebApi,
                    MessageType.BayOperationalStatusChanged,
                    MessageStatus.NoStatus));

            return entry.Entity;
        }

        #endregion
    }
}
