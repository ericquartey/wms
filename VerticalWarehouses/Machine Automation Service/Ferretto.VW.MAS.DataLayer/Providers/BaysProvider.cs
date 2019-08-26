using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer.Providers
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

        public Bay Activate(BayIndex bayIndex)
        {
            var bay = this.GetByIndex(bayIndex);
            if (bay != null)
            {
                bay.IsActive = true;

                this.Update(bay);
            }

            return bay;
        }

        public Bay AssignMissionOperation(BayIndex bayIndex, int? missionId, int? missionOperationId)
        {
            var bay = this.GetByIndex(bayIndex);
            if (bay != null)
            {
                bay.CurrentMissionId = missionId;
                bay.CurrentMissionOperationId = missionOperationId;

                this.Update(bay);
            }

            return bay;
        }

        public void Create(Bay bay)
        {
            this.dataContext.Bays.Add(bay);

            this.dataContext.SaveChanges();
        }

        public Bay Deactivate(BayIndex bayIndex)
        {
            var bay = this.GetByIndex(bayIndex);
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

        public Bay GetByIndex(BayIndex bayIndex)
        {
            return this.dataContext.Bays.SingleOrDefault(b => b.Index == bayIndex);
        }

        public BayIndex GetByInverterIndex(InverterIndex inverterIndex)
        {
            BayIndex returnValue = BayIndex.None;

            switch (inverterIndex)
            {
                case InverterIndex.MainInverter:
                case InverterIndex.Slave1:
                    returnValue = BayIndex.ElevatorBay;
                    break;

                case InverterIndex.Slave2:
                case InverterIndex.Slave3:
                    returnValue = BayIndex.BayOne;
                    break;

                case InverterIndex.Slave4:
                case InverterIndex.Slave5:
                    returnValue = BayIndex.BayTwo;
                    break;

                case InverterIndex.Slave6:
                case InverterIndex.Slave7:
                    returnValue = BayIndex.BayThree;
                    break;
            }

            return returnValue;
        }

        public BayIndex GetByIoIndex(IoIndex ioIndex)
        {
            BayIndex returnValue = BayIndex.None;

            switch (ioIndex)
            {
                case IoIndex.IoDevice1:
                    returnValue = BayIndex.BayOne;
                    break;

                case IoIndex.IoDevice2:
                    returnValue = BayIndex.BayTwo;
                    break;

                case IoIndex.IoDevice3:
                    returnValue = BayIndex.BayThree;
                    break;
            }

            return returnValue;
        }

        public Bay GetByIpAddress(IPAddress remoteIpAddress)
        {
            return this.dataContext.Bays
                .SingleOrDefault(b => b.IpAddress == remoteIpAddress.ToString());
        }

        public List<InverterIndex> GetInverterList(BayIndex bayIndex)
        {
            var returnValue = new List<InverterIndex>();

            var bay = this.GetByIndex(bayIndex);

            switch (bay.Type)
            {
                case BayType.Elevator:
                    //TODO define a way to identify the machine version 1000 Kg or not
                    bool bigMachine = false;
                    if (bigMachine)
                    {
                        returnValue.Add(InverterIndex.MainInverter);
                        returnValue.Add(InverterIndex.Slave1); ;
                    }
                    else
                    {
                        returnValue.Add(InverterIndex.MainInverter);
                    }

                    break;

                case BayType.Single:
                case BayType.Double:
                    returnValue.Add(Enum.Parse<InverterIndex>(((int)bayIndex * 2).ToString()));
                    break;

                case BayType.ExternalSingle:
                case BayType.ExternalDouble:
                case BayType.Carousel:
                    returnValue.Add(Enum.Parse<InverterIndex>(((int)bayIndex * 2).ToString()));
                    returnValue.Add(Enum.Parse<InverterIndex>(((int)bayIndex * 2 + 1).ToString()));
                    break;
            }

            return returnValue;
        }

        public IoIndex GetIoDevice(BayIndex bayIndex)
        {
            IoIndex returnValue = IoIndex.None;

            switch (bayIndex)
            {
                case BayIndex.ElevatorBay:
                case BayIndex.BayOne:
                    returnValue = IoIndex.IoDevice1;
                    break;

                case BayIndex.BayTwo:
                    returnValue = IoIndex.IoDevice2;
                    break;

                case BayIndex.BayThree:
                    returnValue = IoIndex.IoDevice3;
                    break;
            }

            return returnValue;
        }

        public void Update(BayIndex bayIndex, string ipAddress, BayType bayType)
        {
            var bay = this.GetByIndex(bayIndex);
            if (bay != null)
            {
                bay.IpAddress = ipAddress;
                bay.Type = bayType;

                this.Update(bay);
            }
        }

        private Bay Update(Bay bay)
        {
            var entry = this.dataContext.Bays.Update(bay);

            this.dataContext.SaveChanges();

            this.notificationEvent.Publish(
                new NotificationMessage(
                    new BayOperationalStatusChangedMessageData
                    {
                        BayStatus = bay.Status,
                    },
                    $"Bay #{bay.Index} status changed to {bay.Status}",
                    MessageActor.MissionsManager,
                    MessageActor.WebApi,
                    MessageType.BayOperationalStatusChanged,
                    bay.Index));

            return entry.Entity;
        }

        #endregion
    }
}
