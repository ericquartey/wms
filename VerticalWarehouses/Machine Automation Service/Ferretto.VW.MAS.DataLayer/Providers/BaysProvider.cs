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
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class BaysProvider : BaseProvider, IBaysProvider
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer configurationValueManagement;

        private readonly DataLayerContext dataContext;

        private readonly IMachineConfigurationProvider machineConfigurationProvider;

        private readonly NotificationEvent notificationEvent;

        private readonly IVerticalAxisDataLayer verticalAxis;

        #endregion

        #region Constructors

        public BaysProvider(DataLayerContext dataContext,
            IEventAggregator eventAggregator,
            IVerticalAxisDataLayer verticalAxis,
            IMachineConfigurationProvider machineConfigurationProvider,
            IConfigurationValueManagmentDataLayer configurationValueManagement)
            : base(eventAggregator)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));

            this.machineConfigurationProvider = machineConfigurationProvider ?? throw new ArgumentNullException(nameof(machineConfigurationProvider));

            this.verticalAxis = verticalAxis;
            this.configurationValueManagement = configurationValueManagement;
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

        public Bay Activate(BayNumber bayIndex)
        {
            var bay = this.GetByIndex(bayIndex);
            if (bay is null)
            {
                throw new EntityNotFoundException(bayIndex);
            }

            bay.IsActive = true;

            this.Update(bay);

            return bay;
        }

        public Bay AssignMissionOperation(BayNumber bayIndex, int? missionId, int? missionOperationId)
        {
            var bay = this.GetByIndex(bayIndex);
            if (bay is null)
            {
                throw new EntityNotFoundException(bayIndex);
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

        public Bay Deactivate(BayNumber bayIndex)
        {
            var bay = this.GetByIndex(bayIndex);
            if (bay is null)
            {
                throw new EntityNotFoundException(bayIndex);
            }

            bay.IsActive = false;

            this.Update(bay);

            return bay;
        }

        public IEnumerable<Bay> GetAll()
        {
            return this.dataContext.Bays.ToArray();
        }

        public Bay GetByIndex(BayNumber bayIndex)
        {
            var bay = this.dataContext.Bays.SingleOrDefault(b => b.ExternalId == (int)bayIndex);
            if (bay is null)
            {
                throw new EntityNotFoundException(bayIndex);
            }

            this.UpdateBayWithPositions(bay);

            return bay;
        }

        public BayNumber GetByInverterIndex(InverterIndex inverterIndex)
        {
            BayNumber returnValue = BayNumber.None;

            switch (inverterIndex)
            {
                case InverterIndex.MainInverter:
                case InverterIndex.Slave1:
                    returnValue = BayNumber.ElevatorBay;
                    break;

                case InverterIndex.Slave2:
                case InverterIndex.Slave3:
                    returnValue = BayNumber.BayOne;
                    break;

                case InverterIndex.Slave4:
                case InverterIndex.Slave5:
                    returnValue = BayNumber.BayTwo;
                    break;

                case InverterIndex.Slave6:
                case InverterIndex.Slave7:
                    returnValue = BayNumber.BayThree;
                    break;
            }

            return returnValue;
        }

        public BayNumber GetByIoIndex(IoIndex ioIndex, FieldMessageType messageType)
        {
            BayNumber returnValue = BayNumber.None;

            //Hack required to handle exceptions (like axis switch on 800Kg machine) in order to fix device/bay association
            if (messageType == FieldMessageType.SwitchAxis)
            {
                returnValue = BayNumber.ElevatorBay;
            }
            else
            {
                switch (ioIndex)
                {
                    case IoIndex.IoDevice1:
                        returnValue = BayNumber.BayOne;
                        break;

                    case IoIndex.IoDevice2:
                        returnValue = BayNumber.BayTwo;
                        break;

                    case IoIndex.IoDevice3:
                        returnValue = BayNumber.BayThree;
                        break;
                }
            }
            return returnValue;
        }

        public Bay GetByIpAddress(IPAddress remoteIpAddress)
        {
            return this.dataContext.Bays
                .SingleOrDefault(b => b.IpAddress == remoteIpAddress.ToString());
        }

        public BayNumber GetByMovementType(IPositioningMessageData data)
        {
            BayNumber targetBay;
            switch (data.MovementMode)
            {
                case MovementMode.BeltBurnishing:
                case MovementMode.FindZero:
                case MovementMode.TorqueCurrentSampling:
                    targetBay = BayNumber.ElevatorBay;
                    break;

                case MovementMode.Position:
                    switch (data.AxisMovement)
                    {
                        case Axis.Horizontal:
                        case Axis.Vertical:
                        case Axis.HorizontalAndVertical:
                            targetBay = BayNumber.ElevatorBay;
                            break;

                        default:
                            targetBay = BayNumber.None;
                            break;
                    }

                    break;

                default:
                    targetBay = BayNumber.None;
                    break;
            }

            return targetBay;
        }

        public InverterIndex GetInverterIndexByMovementType(IPositioningMessageData data, BayNumber bayIndex)
        {
            InverterIndex returnValue = InverterIndex.None;

            switch (bayIndex)
            {
                case BayNumber.ElevatorBay:
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
                        case MovementMode.TorqueCurrentSampling:
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

                case BayNumber.BayOne:
                case BayNumber.BayTwo:
                case BayNumber.BayThree:
                    switch (data.MovementMode)
                    {
                        case MovementMode.ShutterTest:
                        case MovementMode.ShutterPosition:
                            returnValue = this.GetInverterList(bayIndex)[this.ShutterInverterPosition];
                            break;

                        case MovementMode.BayChain:
                        case MovementMode.BayChainManual:
                        case MovementMode.BayTest:
                            returnValue = this.GetInverterList(bayIndex)[this.BayInverterPosition];
                            break;

                        default:
                            break;
                    }
                    break;

                default:
                    break;
            }

            return returnValue;
        }

        public List<InverterIndex> GetInverterList(BayNumber bayIndex)
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

        public IoIndex GetIoDevice(BayNumber bayIndex)
        {
            IoIndex returnValue = IoIndex.None;

            switch (bayIndex)
            {
                case BayNumber.ElevatorBay:
                case BayNumber.BayOne:
                    returnValue = IoIndex.IoDevice1;
                    break;

                case BayNumber.BayTwo:
                    returnValue = IoIndex.IoDevice2;
                    break;

                case BayNumber.BayThree:
                    returnValue = IoIndex.IoDevice3;
                    break;
            }

            return returnValue;
        }

        public Bay SetCurrentOperation(BayNumber targetBay, BayOperation newOperation)
        {
            var bay = this.GetByIndex(targetBay);
            if (bay is null)
            {
                throw new EntityNotFoundException(targetBay);
            }

            bay.Operation = newOperation;

            this.Update(bay);

            return bay;
        }

        public void Update(BayNumber bayIndex, string ipAddress, BayType bayType)
        {
            var bay = this.GetByIndex(bayIndex);
            if (bay is null)
            {
                throw new EntityNotFoundException(bayIndex);
            }

            bay.IpAddress = ipAddress;
            bay.Type = bayType;

            this.Update(bay);
        }

        public Bay UpdatePosition(BayNumber bayIndex, int position, decimal height)
        {
            var bay = this.GetByIndex(bayIndex);
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

            var bayPosition = $"Bay{(int)bayIndex}Position{position}";
            if (Enum.TryParse<GeneralInfo>(bayPosition, out var positionValue))
            {
                this.configurationValueManagement.SetDecimalConfigurationValue((long)positionValue, ConfigurationCategory.GeneralInfo, height);

                return this.GetByIndex(bayIndex);
            }
            else
            {
                throw new ArgumentOutOfRangeException(Resources.Bays.TheSpecifiedBayPositionIsNotValid);
            }
        }

        private void Update(Bay bay)
        {
            if (bay is null)
            {
                throw new ArgumentNullException(nameof(bay));
            }

            this.dataContext.Bays.Update(bay);

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
                var bayPosition = $"Bay{(int)bay.Index}Position{position}";
                if (Enum.TryParse<GeneralInfo>(bayPosition, out var positionFound))
                {
                    try
                    {
                        positions.Add(
                            this.configurationValueManagement
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
