using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Exceptions;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.EntityFrameworkCore;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal sealed class BaysProvider : BaseProvider, IBaysProvider
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer configurationValueManagement;

        private readonly DataLayerContext dataContext;

        private readonly IDigitalDevicesDataProvider digitalDevicesDataProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IMachineProvider machineProvider;

        private readonly NotificationEvent notificationEvent;

        #endregion

        #region Constructors

        public BaysProvider(
            DataLayerContext dataContext,
            IEventAggregator eventAggregator,
            IMachineProvider machineProvider,
            IDigitalDevicesDataProvider digitalDevicesDataProvider,
            IElevatorDataProvider elevatorDataProvider,
            IConfigurationValueManagmentDataLayer configurationValueManagement)
            : base(eventAggregator)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.digitalDevicesDataProvider = digitalDevicesDataProvider ?? throw new ArgumentNullException(nameof(digitalDevicesDataProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.configurationValueManagement = configurationValueManagement ?? throw new ArgumentNullException(nameof(configurationValueManagement));

            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
        }

        #endregion

        #region Methods

        public Bay Activate(BayNumber bayIndex)
        {
            var bay = this.GetByNumber(bayIndex);
            if (bay is null)
            {
                throw new EntityNotFoundException(bayIndex.ToString());
            }

            bay.IsActive = true;

            this.Update(bay);

            return bay;
        }

        public void AddElevatorPseudoBay()
        {
            if (this.dataContext.Bays.Any(b => b.Number == BayNumber.ElevatorBay))
            {
                return;
            }

            this.dataContext.Bays.Add(new Bay
            {
                Number = BayNumber.ElevatorBay,
            });
            this.dataContext.SaveChanges();
        }

        public Bay AssignMissionOperation(BayNumber bayIndex, int? missionId, int? missionOperationId)
        {
            var bay = this.GetByNumber(bayIndex);
            if (bay is null)
            {
                throw new EntityNotFoundException(bayIndex.ToString());
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
            var bay = this.GetByNumber(bayIndex);
            if (bay is null)
            {
                throw new EntityNotFoundException(bayIndex.ToString());
            }

            bay.IsActive = false;

            this.Update(bay);

            return bay;
        }

        public IEnumerable<Bay> GetAll()
        {
            return this.dataContext.Bays.ToArray();
        }

        public BayNumber GetByInverterIndex(InverterIndex inverterIndex)
        {
            var returnValue = BayNumber.None;

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
            var returnValue = BayNumber.None;

            // Hack required to handle exceptions (like axis switch on 800Kg machine) in order to fix device/bay association
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

        public BayNumber GetByMovementType(IPositioningMessageData data)
        {
            BayNumber targetBay;
            switch (data.MovementMode)
            {
                case MovementMode.BeltBurnishing:
                case MovementMode.FindZero:
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

        public Bay GetByNumber(BayNumber bayNumber)
        {
            var bay = this.dataContext.Bays
                .Include(b => b.Inverter)
                .Include(b => b.Positions)
                .Include(b => b.Shutter)
                .ThenInclude(s => s.Inverter)
                .Include(b => b.LoadingUnit)
                .SingleOrDefault(b => b.Number == bayNumber);

            if (bay is null)
            {
                throw new EntityNotFoundException(bayNumber.ToString());
            }

            return bay;
        }

        public InverterIndex GetInverterIndexByMovementType(IPositioningMessageData data, BayNumber bayIndex)
        {
            var returnValue = InverterIndex.None;

            switch (bayIndex)
            {
                case BayNumber.ElevatorBay:
                    switch (data.MovementMode)
                    {
                        case MovementMode.Position:
                            switch (data.AxisMovement)
                            {
                                case Axis.Horizontal:
                                    returnValue = this.machineProvider.IsOneTonMachine() ? InverterIndex.Slave1 : InverterIndex.MainInverter;
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
                            returnValue = this.machineProvider.IsOneTonMachine() ? InverterIndex.Slave1 : InverterIndex.MainInverter;
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
                            returnValue = this.GetByNumber(bayIndex).Shutter.Inverter.Index;
                            break;

                        case MovementMode.BayChain:
                        case MovementMode.BayChainManual:
                        case MovementMode.BayTest:
                            returnValue = this.GetByNumber(bayIndex).Inverter.Index;
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

        public IoIndex GetIoDevice(BayNumber bayIndex)
        {
            var returnValue = IoIndex.None;

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
            var bay = this.GetByNumber(targetBay);
            if (bay is null)
            {
                throw new EntityNotFoundException(targetBay.ToString());
            }

            bay.Operation = newOperation;

            this.Update(bay);

            return bay;
        }

        public Bay UpdatePosition(BayNumber bayNumber, int positionIndex, double height)
        {
            var bay = this.GetByNumber(bayNumber);
            if (positionIndex < 0 || positionIndex > bay.Positions.Count() - 1)
            {
                throw new ArgumentOutOfRangeException(Resources.Bays.TheSpecifiedBayPositionIsNotValid);
            }

            var verticalAxis = this.elevatorDataProvider.GetVerticalAxis();
            if (height < verticalAxis.LowerBound || height > verticalAxis.UpperBound)
            {
                throw new ArgumentOutOfRangeException(
                    string.Format(Resources.Bays.TheBayHeightMustBeInRange, height, verticalAxis.LowerBound, verticalAxis.UpperBound));
            }

            var position = positionIndex == 0 ? bay.Positions.First() : bay.Positions.Last();

            position.Height = height;

            this.dataContext.SaveChanges();

            return this.GetByNumber(bayNumber);
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
                    $"Bay #{bay.Number} status changed to {bay.Status}",
                    MessageActor.MissionsManager,
                    MessageActor.WebApi,
                    MessageType.BayOperationalStatusChanged,
                    bay.Number));
        }

        #endregion
    }
}
