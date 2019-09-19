using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Exceptions;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class BaysProvider : Interfaces.IBaysProvider
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer configurationValueManagment;

        private readonly DataLayerContext dataContext;

        private readonly NotificationEvent notificationEvent;

        private readonly IVerticalAxisDataLayer verticalAxis;

        #endregion

        #region Constructors

        public BaysProvider(DataLayerContext dataContext,
                            IEventAggregator eventAggregator,
                            IVerticalAxisDataLayer verticalAxis,
                            IConfigurationValueManagmentDataLayer configurationValueManagment)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (dataContext is null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            if (verticalAxis is null)
            {
                throw new ArgumentNullException(nameof(verticalAxis));
            }

            if (configurationValueManagment is null)
            {
                throw new ArgumentNullException(nameof(configurationValueManagment));
            }

            this.dataContext = dataContext;
            this.verticalAxis = verticalAxis;
            this.configurationValueManagment = configurationValueManagment;
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

            this.UpdateBayWithPositions(bay);

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

        public Bay UpdatePosition(int bayNumber, int position, decimal height)
        {
            var bay = this.GetByNumber(bayNumber);
            if (bay.Positions.Count() < position)
            {
                throw new ArgumentOutOfRangeException(Resources.Bays.TheSpecifiedBayPositionIsNotValid);
            }

            var lowerBound = this.verticalAxis.LowerBound;
            var upperBound = this.verticalAxis.UpperBound;
            if (height < lowerBound || height > upperBound)
            {
                throw new ArgumentOutOfRangeException(
                    string.Format(Resources.Bays.TheBayHeightMustBeInRange, height, lowerBound, upperBound));
            }

            var bayPosition = $"Bay{bayNumber}Position{position}";
            if (Enum.TryParse<GeneralInfo>(bayPosition, out var positionValue))
            {
                this.configurationValueManagment.SetDecimalConfigurationValue((long)positionValue, ConfigurationCategory.GeneralInfo, height);

                return this.GetByNumber(bayNumber);
            }
            else
            {
                throw new ArgumentOutOfRangeException(Resources.Bays.TheSpecifiedBayPositionIsNotValid);
            }
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

        private void UpdateBayWithPositions(Bay bay)
        {
            if (bay is null)
            {
                throw new ArgumentNullException(nameof(bay));
            }

            var positions = new List<decimal>();

            for (var position = 1; position <= 2; position++)
            {
                var bayPosition = $"Bay{bay.Number}Position{position}";
                if (Enum.TryParse<GeneralInfo>(bayPosition, out var positionFound))
                {
                    try
                    {
                        positions.Add(
                            this.configurationValueManagment
                                .GetDecimalConfigurationValue(positionFound, ConfigurationCategory.GeneralInfo));
                    }
                    catch
                    {
                    }
                }
            }

            bay.Positions = positions;
        }

        #endregion
    }
}
