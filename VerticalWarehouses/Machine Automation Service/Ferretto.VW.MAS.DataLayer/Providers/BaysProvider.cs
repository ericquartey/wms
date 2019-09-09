using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Exceptions;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class BaysProvider : IBaysProvider
    {

        #region Fields

        private readonly IConfigurationValueManagmentDataLayer configurationValueManagment;

        private readonly DataLayerContext dataContext;

        private readonly IMachineConfigurationProvider machineConfigurationProvider;

        private readonly NotificationEvent notificationEvent;

        private readonly IVerticalAxisDataLayer verticalAxis;

        #endregion

        #region Constructors

        public BaysProvider(DataLayerContext dataContext, IMachineConfigurationProvider machineConfigurationProvider, IEventAggregator eventAggregator)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));

            this.machineConfigurationProvider = machineConfigurationProvider ?? throw new ArgumentNullException(nameof(machineConfigurationProvider));

            this.verticalAxis = verticalAxis;
            this.configurationValueManagment = configurationValueManagment;
            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
        }

        #endregion



        #region Properties

        /// <summary>
        /// Helper property used to identify Chain/Carouser inverter in Inverter list
        /// </summary>
        public int BayInverterPosition => 1;

        /// <summary>
        /// Helper property used to identify Shutter inverter in Inverter list
        /// </summary>
        public int ShutterInverterPosition => 0;

        #endregion



        #region Methods

        public Bay Activate(BayIndex bayIndex)
        {
            var bay = this.GetByIndex(bayIndex);
            if (bay != null)
            {
                throw new EntityNotFoundException(bayNumber);
            }

            bay.IsActive = true;

            this.Update(bay);

            return bay;
        }

        public Bay AssignMissionOperation(BayIndex bayIndex, int? missionId, int? missionOperationId)
        {
            var bay = this.GetByIndex(bayIndex);
            if (bay != null)
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

        public Bay Deactivate(BayIndex bayIndex)
        {
            var bay = this.GetByIndex(bayIndex);
            if (bay != null)
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

        public BayIndex GetByMovementType(IPositioningMessageData data)
        {
            BayIndex targetBay;
            switch (data.MovementMode)
            {
                case MovementMode.BeltBurnishing:
                case MovementMode.FindZero:
                    targetBay = BayIndex.ElevatorBay;
                    break;

                case MovementMode.Position:
                    switch (data.AxisMovement)
                    {
                        case Axis.Horizontal:
                        case Axis.Vertical:
                        case Axis.HorizontalAndVertical:
                            targetBay = BayIndex.ElevatorBay;
                            break;

                        default:
                            targetBay = BayIndex.None;
                            break;
                    }

                    break;

                default:
                    targetBay = BayIndex.None;
                    break;
            }

            return targetBay;
        }

        public InverterIndex GetInverterIndexByMovementType(IPositioningMessageData data, BayIndex bayIndex)
        {
            InverterIndex returnValue = InverterIndex.None;

            switch (bayIndex)
            {
                case BayIndex.ElevatorBay:
                    switch (data.MovementMode)
                    {
                        case MovementMode.Position:
                            switch (data.AxisMovement)
                            {
                                case Axis.Horizontal:
                                    returnValue = this.machineConfigurationProvider.IsOneKMachine() ? InverterIndex.Slave1 : InverterIndex.MainInverter;
                                    break;

                                case Axis.Vertical:
                                    returnValue = InverterIndex.MainInverter;
                                    break;

                                default:
                                    throw new InvalidOperationException("Axis.HorizontalAndVertical is not a valid Axis for GetInverterIndexByMovementType() method");
                            }
                            break;

                        case MovementMode.BeltBurnishing:
                            returnValue = InverterIndex.MainInverter;
                            break;

                        case MovementMode.FindZero:
                        case MovementMode.Profile:
                            returnValue = this.machineConfigurationProvider.IsOneKMachine() ? InverterIndex.Slave1 : InverterIndex.MainInverter;
                            break;

                        default:
                            returnValue = InverterIndex.None;
                            break;
                    }
                    break;

                case BayIndex.BayOne:
                case BayIndex.BayTwo:
                case BayIndex.BayThree:
                    switch (data.MovementMode)
                    {
                        case MovementMode.ShutterTest:
                        case MovementMode.Position:
                            returnValue = Enum.Parse<InverterIndex>(((int)bayIndex * 2).ToString());
                            break;
                    }
                    break;

                default:
                    returnValue = InverterIndex.None;
                    break;
            }

            return returnValue;
        }

        public List<InverterIndex> GetInverterList(BayIndex bayIndex)
        {
            var returnValue = new List<InverterIndex>();

            var bay = this.GetByIndex(bayIndex);

            switch (bay.Type)
            {
                case BayType.Elevator:
                    if (this.machineConfigurationProvider.IsOneKMachine())
                    {
                        returnValue.Add(InverterIndex.MainInverter);
                        returnValue.Add(InverterIndex.Slave1);
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
                        BayStatus = bay.Status,
                    },
                    $"Bay #{bay.Index} status changed to {bay.Status}",
                    MessageActor.MissionsManager,
                    MessageActor.WebApi,
                    MessageType.BayOperationalStatusChanged,
                    bay.Index));

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
                                .GetDecimalConfigurationValue((long)positionFound, ConfigurationCategory.GeneralInfo));
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
