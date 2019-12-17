using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class BaysDataProvider : BaseProvider, IBaysDataProvider
    {
        #region Fields

        private readonly IBayChainVolatileDataProvider bayChainVolatileDataProvider;

        private readonly IMemoryCache cache;

        private readonly MemoryCacheEntryOptions cacheOptions;

        private readonly DataLayerContext dataContext;

        private readonly IElevatorDataProvider elevatorDataProvider;

        /// <summary>
        /// TODO move to configuration
        /// </summary>
        private readonly double kMul = 0.090625;

        /// <summary>
        /// TODO move to configuration
        /// </summary>
        private readonly double kSum = -181.25;

        private readonly ILogger<DataLayerContext> logger;

        private readonly IMachineProvider machineProvider;

        private readonly NotificationEvent notificationEvent;

        #endregion

        #region Constructors

        public BaysDataProvider(
            DataLayerContext dataContext,
            IEventAggregator eventAggregator,
            IMachineProvider machineProvider,
            IConfiguration configuration,
            IElevatorDataProvider elevatorDataProvider,
            IBayChainVolatileDataProvider bayChainVolatileDataProvider,
            IMemoryCache memoryCache,
            ILogger<DataLayerContext> logger)
            : base(eventAggregator)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.bayChainVolatileDataProvider = bayChainVolatileDataProvider ?? throw new ArgumentNullException(nameof(bayChainVolatileDataProvider));
            this.cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
            this.cacheOptions = configuration.GetMemoryCacheOptions();
        }

        #endregion

        #region Methods

        public Bay Activate(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                var bay = this.GetByNumber(bayNumber);
                if (bay is null)
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }

                if (!bay.IsActive)
                {
                    bay.IsActive = true;

                    this.logger.LogInformation($"The bay {bay.Number} is now active and ready to accept missions.");
                }

                this.Update(bay);

                return bay;
            }
        }

        public void AddElevatorPseudoBay()
        {
            lock (this.dataContext)
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
        }

        public Bay AssignMission(BayNumber bayNumber, Mission mission)
        {
            if (mission is null)
            {
                throw new ArgumentNullException(nameof(mission));
            }

            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.CurrentMission)
                    .SingleOrDefault(b => b.Number == bayNumber);

                if (bay is null)
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }

                bay.CurrentMission = this.dataContext.Missions.SingleOrDefault(m => m.Id == mission.Id) ?? mission;
                bay.CurrentWmsMissionOperationId = null;

                this.Update(bay);

                return bay;
            }
        }

        public Bay AssignWmsMission(BayNumber bayNumber, Mission mission, int? wmsMissionOperationId)
        {
            if (mission is null)
            {
                throw new ArgumentNullException(nameof(mission));
            }

            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.CurrentMission)
                    .SingleOrDefault(b => b.Number == bayNumber);

                if (bay is null)
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }

                bay.CurrentMission = this.dataContext.Missions.SingleOrDefault(m => m.Id == mission.Id) ?? mission;
                bay.CurrentWmsMissionOperationId = wmsMissionOperationId;

                this.Update(bay);

                return bay;
            }
        }

        public Bay ClearMission(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays.SingleOrDefault(b => b.Number == bayNumber);
                if (bay is null)
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }

                bay.CurrentMission = null;
                bay.CurrentWmsMissionOperationId = null;

                this.Update(bay);

                return bay;
            }
        }

        /// <summary>
        /// <param name="profile">value read from inverter</param>
        /// profile = 200   ==> height = 0
        /// profile = 10000 ==> height = 725mm
        /// height = kMul * profile + kSum;
        /// </summary>
        [Obsolete("This method contains business logic. It should not be in the DataLayer.")]
        public double ConvertProfileToHeight(ushort profile)
        {
            return (profile * this.kMul) + this.kSum;
        }

        [Obsolete("This method contains business logic. It should not be in the DataLayer.")]
        public double ConvertPulsesToMillimeters(double pulses, InverterIndex inverterIndex)
        {
            if (pulses == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pulses), "Pulses must be different from zero.");
            }

            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .SingleOrDefault(b => b.Inverter.Index == inverterIndex);

                if (bay is null)
                {
                    throw new EntityNotFoundException(inverterIndex.ToString());
                }

                if (bay.Resolution == 0)
                {
                    throw new InvalidOperationException(
                        $"Configured inverter {inverterIndex} resolution is zero, therefore it is not possible to convert pulses to millimeters.");
                }

                return pulses / bay.Resolution;
            }
        }

        public Bay Deactivate(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                var bay = this.GetByNumber(bayNumber);
                if (bay is null)
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }

                if (bay.IsActive)
                {
                    bay.IsActive = false;

                    this.Update(bay);

                    this.logger.LogInformation($"The bay {bay.Number} is now deactivated and can no longer accept missions.");
                }

                return bay;
            }
        }

        public void FindZero(BayNumber bayNumber)
        {
            this.PublishCommand(
                new HomingMessageData(Axis.BayChain, Calibration.FindSensor, null),
                "Execute FindZeroSensor Command",
                MessageActor.DeviceManager,
                MessageType.Homing,
                bayNumber,
                bayNumber);
        }

        public IEnumerable<Bay> GetAll()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Bays
                    .AsNoTracking()
                    .Include(b => b.Inverter)
                    .Include(b => b.Positions)
                        .ThenInclude(s => s.LoadingUnit)
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.AssistedMovements)
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.ManualMovements)
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.Inverter)
                    .Include(b => b.Carousel)
                    .Include(b => b.EmptyLoadMovement)
                    .Include(b => b.FullLoadMovement)
                    .ToArray();
            }
        }

        public CarouselManualParameters GetAssistedMovementsCarousel(BayNumber bayNumber) => this.GetByNumber(bayNumber).Carousel.AssistedMovements;

        public ShutterManualParameters GetAssistedMovementsShutter(BayNumber bayNumber) => this.GetByNumber(bayNumber).Shutter.AssistedMovements;

        [Obsolete("This method contains business logic. It should not be in the DataLayer.")]
        public BayNumber GetByAxis(IHomingMessageData data)
        {
            BayNumber targetBay;
            switch (data.AxisToCalibrate)
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

            return targetBay;
        }

        public Bay GetByBayPositionId(int bayPositionId)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays.SingleOrDefault(b => b.Positions.Any(p => p.Id == bayPositionId));
                if (bay is null)
                {
                    throw new EntityNotFoundException(bayPositionId);
                }

                return bay;
            }
        }

        public Bay GetByIdOrDefault(int id)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Bays.AsNoTracking().SingleOrDefault(b => b.Id == id);
            }
        }

        public BayNumber GetByInverterIndex(InverterIndex inverterIndex)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays.AsNoTracking().SingleOrDefault(b =>
                    b.Inverter.Index == inverterIndex
                    ||
                    b.Shutter.Inverter.Index == inverterIndex);

                if (bay is null)
                {
                    if (this.GetElevatorAxes().Any(a => a.Inverter.Index == inverterIndex))
                    {
                        return BayNumber.ElevatorBay;
                    }
                    else
                    {
                        throw new EntityNotFoundException(inverterIndex.ToString());
                    }
                }

                return bay.Number;
            }
        }

        [Obsolete("This method contains business logic. It should not be in the DataLayer.")]
        public BayNumber GetByIoIndex(IoIndex ioIndex, FieldMessageType messageType)
        {
            // Hack required to handle exceptions (like axis switch on 800Kg machine) in order to fix device/bay association
            if (messageType is FieldMessageType.SwitchAxis)
            {
                return BayNumber.ElevatorBay;
            }

            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .AsNoTracking()
                    .SingleOrDefault(b => b.IoDevice.Index == ioIndex);

                if (bay is null)
                {
                    throw new EntityNotFoundException(ioIndex.ToString());
                }

                return bay.Number;
            }
        }

        public Bay GetByIoIndex(IoIndex ioIndex)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .AsNoTracking()
                    .Include(b => b.Inverter)
                    .Include(b => b.Positions)
                        .ThenInclude(s => s.LoadingUnit)
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.Inverter)
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.ManualMovements)
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.AssistedMovements)
                    .Include(b => b.Carousel)
                    .Include(b => b.EmptyLoadMovement)
                    .Include(b => b.FullLoadMovement)
                    .SingleOrDefault(b => b.IoDevice.Index == ioIndex);

                if (bay is null)
                {
                    throw new EntityNotFoundException(ioIndex.ToString());
                }

                return bay;
            }
        }

        public Bay GetByLoadingUnitLocation(LoadingUnitLocation location)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Bays
                    .AsNoTracking()
                     .Include(b => b.Shutter)
                     .Include(b => b.Carousel)
                     .Include(b => b.Positions)
                    .FirstOrDefault(b => b.Positions.Any(p => p.Location == location));
            }
        }

        [Obsolete("This method contains business logic. It should not be in the DataLayer.")]
        public BayNumber GetByMovementType(IPositioningMessageData data)
        {
            BayNumber targetBay;
            switch (data.MovementMode)
            {
                case MovementMode.BeltBurnishing:
                case MovementMode.FindZero:
                case MovementMode.TorqueCurrentSampling:
                case MovementMode.ProfileCalibration:
                    targetBay = BayNumber.ElevatorBay;
                    break;

                case MovementMode.Position:
                case MovementMode.PositionAndMeasureProfile:
                case MovementMode.PositionAndMeasureWeight:
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
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Inverter)
                    .Include(b => b.Positions)
                        .ThenInclude(s => s.LoadingUnit)
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.Inverter)
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.AssistedMovements)
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.ManualMovements)
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.Inverter)
                    .Include(b => b.Carousel)
                        .ThenInclude(s => s.ManualMovements)
                    .Include(b => b.Carousel)
                        .ThenInclude(s => s.AssistedMovements)
                    .Include(b => b.EmptyLoadMovement)
                    .Include(b => b.FullLoadMovement)
                    .SingleOrDefault(b => b.Number == bayNumber);

                if (bay is null)
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }

                return bay;
            }
        }

        public double GetChainOffset(InverterIndex inverterIndex)
        {
            lock (this.dataContext)
            {
                try
                {
                    return this.dataContext.Bays
                        .AsNoTracking()
                        .Where(b => b.Inverter.Index == inverterIndex)
                        .Select(b => b.ChainOffset)
                        .Single();
                }
                catch
                {
                    throw new EntityNotFoundException(inverterIndex.ToString());
                }
            }
        }

        public double GetChainPosition(BayNumber bayNumber)
        {
            return this.bayChainVolatileDataProvider.GetPositionByBayNumber(bayNumber);
        }

        public IEnumerable<ElevatorAxis> GetElevatorAxes()
        {
            lock (this.dataContext)
            {
                var cacheKey = GetElevatorAxesCacheKey();
                if (!this.cache.TryGetValue(cacheKey, out IEnumerable<ElevatorAxis> cacheEntry))
                {
                    cacheEntry = this.dataContext.ElevatorAxes
                        .AsNoTracking()
                        .Include(i => i.Inverter)
                        .ToList();

                    if (cacheEntry is null)
                    {
                        throw new EntityNotFoundException(string.Empty);
                    }

                    this.cache.Set(cacheKey, cacheEntry, this.cacheOptions);
                }

                return cacheEntry;
            }
        }

        [Obsolete("This method contains business logic. It should not be in the DataLayer.")]
        public InverterIndex GetInverterIndexByAxis(Axis axis, BayNumber bayNumber)
        {
            var returnValue = InverterIndex.None;

            switch (bayNumber)
            {
                case BayNumber.ElevatorBay:
                    returnValue = InverterIndex.MainInverter;
                    break;

                case BayNumber.BayOne:
                case BayNumber.BayTwo:
                case BayNumber.BayThree:
                    returnValue = this.GetByNumber(bayNumber).Inverter.Index;
                    break;

                default:
                    break;
            }

            return returnValue;
        }

        [Obsolete("This method contains business logic. It should not be in the DataLayer.")]
        public InverterIndex GetInverterIndexByMovementType(IPositioningMessageData data, BayNumber bayNumber)
        {
            var returnValue = InverterIndex.None;

            switch (bayNumber)
            {
                case BayNumber.ElevatorBay:
                    switch (data.MovementMode)
                    {
                        case MovementMode.Position:
                        case MovementMode.PositionAndMeasureProfile:
                        case MovementMode.PositionAndMeasureWeight:
                        case MovementMode.ProfileCalibration:
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
                        case MovementMode.TorqueCurrentSampling:
                            returnValue = InverterIndex.MainInverter;
                            break;

                        case MovementMode.FindZero:
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
                            returnValue = this.GetByNumber(bayNumber).Shutter.Inverter.Index;
                            break;

                        case MovementMode.BayChain:
                        case MovementMode.BayChainManual:
                        case MovementMode.BayTest:
                            returnValue = this.GetByNumber(bayNumber).Inverter.Index;
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

        [Obsolete("This method contains business logic. It should not be in the DataLayer.")]
        public InverterIndex GetInverterIndexByProfile(BayNumber bayNumber)
        {
            var returnValue = InverterIndex.None;

            switch (bayNumber)
            {
                case BayNumber.BayOne:
                    returnValue = InverterIndex.MainInverter;
                    break;

                case BayNumber.BayTwo:
                case BayNumber.BayThree:
                    returnValue = this.GetByNumber(bayNumber).Shutter.Inverter.Index;
                    break;

                default:
                    break;
            }

            return returnValue;
        }

        [Obsolete("This method contains business logic. It should not be in the DataLayer.")]
        public IoIndex GetIoDevice(BayNumber bayNumber)
        {
            var returnValue = IoIndex.None;

            switch (bayNumber)
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

        public LoadingUnit GetLoadingUnitByDestination(LoadingUnitLocation location)
        {
            lock (this.dataContext)
            {
                return this.dataContext.BayPositions
                    .AsNoTracking()
                    .Where(p => p.Location == location)
                    .Select(p => p.LoadingUnit)
                    .SingleOrDefault();
            }
        }

        public double? GetLoadingUnitDestinationHeight(LoadingUnitLocation location)
        {
            lock (this.dataContext)
            {
                return this.dataContext.BayPositions
                    .AsNoTracking()
                    .Where(p => p.Location == location)
                    .Select(l => l.Height)
                    .SingleOrDefault();
            }
        }

        public LoadingUnitLocation GetLoadingUnitLocationByLoadingUnit(int loadingUnitId)
        {
            lock (this.dataContext)
            {
                return this.dataContext.BayPositions
                    .AsNoTracking()
                    .SingleOrDefault(p => p.LoadingUnit.Id == loadingUnitId)?.Location ?? LoadingUnitLocation.NoLocation;
            }
        }

        public CarouselManualParameters GetManualMovementsCarousel(BayNumber bayNumber) => this.GetByNumber(bayNumber).Carousel.ManualMovements;

        public ShutterManualParameters GetManualMovementsShutter(BayNumber bayNumber) => this.GetByNumber(bayNumber).Shutter.ManualMovements;

        public BayPosition GetPositionById(int bayPositionId)
        {
            lock (this.dataContext)
            {
                var bayPosition = this.dataContext.BayPositions
                    .Include(b => b.LoadingUnit)
                    .SingleOrDefault(p => p.Id == bayPositionId);

                if (bayPosition is null)
                {
                    throw new EntityNotFoundException(bayPositionId);
                }

                return bayPosition;
            }
        }

        public BayPosition GetPositionByLocation(LoadingUnitLocation location)
        {
            var bayPosition = this.dataContext.BayPositions
                .Include(b => b.Bay)
                .SingleOrDefault(p => p.Location == location);
            if (bayPosition is null)
            {
                throw new EntityNotFoundException(location.ToString());
            }

            return bayPosition;
        }

        public double GetResolution(InverterIndex inverterIndex)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .AsNoTracking()
                    .SingleOrDefault(b => b.Inverter.Index == inverterIndex);

                if (bay is null)
                {
                    throw new EntityNotFoundException(inverterIndex.ToString());
                }

                return bay.Resolution;
            }
        }

        public void PerformHoming(BayNumber bayNumber)
        {
            this.PublishCommand(
                new HomingMessageData(Axis.BayChain, Calibration.ResetEncoder, null),
                "Execute Homing Command",
                MessageActor.DeviceManager,
                MessageType.Homing,
                bayNumber,
                bayNumber);
        }

        public void RemoveLoadingUnit(int loadingUnitId)
        {
            var lu = this.dataContext.LoadingUnits.SingleOrDefault(p => p.Id.Equals(loadingUnitId));
            if (lu is null)
            {
                throw new EntityNotFoundException($"LoadingUnit ID={loadingUnitId}");
            }

            this.dataContext.LoadingUnits.Remove(lu);
            this.dataContext.SaveChanges();
        }

        public void ResetMachine()
        {
            lock (this.dataContext)
            {
                foreach (var bayPosition in this.dataContext.BayPositions
                                            .Include(i => i.LoadingUnit))
                {
                    if (bayPosition.LoadingUnit != null)
                    {
                        bayPosition.LoadingUnit = null;
                        this.dataContext.BayPositions.Update(bayPosition);
                    }
                }

                foreach (var bay in this.dataContext.Bays)
                {
                    bay.CurrentMission = null;
                    bay.CurrentWmsMissionOperationId = null;
                    this.Update(bay);
                }
            }
        }

        public void SetChainPosition(BayNumber bayNumber, double value)
        {
            this.bayChainVolatileDataProvider.SetPosition(bayNumber, value);
        }

        public Bay SetCurrentOperation(BayNumber targetBay, BayOperation newOperation)
        {
            lock (this.dataContext)
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
        }

        public void SetLoadingUnit(int bayPositionId, int? loadingUnitId)
        {
            var position = this.dataContext.BayPositions.SingleOrDefault(p => p.Id == bayPositionId);
            if (position is null)
            {
                throw new EntityNotFoundException($"BayPosition ID={bayPositionId}");
            }

            if (loadingUnitId is null)
            {
                position.LoadingUnit = null;
            }
            else
            {
                var loadingUnit = this.dataContext.LoadingUnits.SingleOrDefault(l => l.Id == loadingUnitId);
                if (loadingUnit is null)
                {
                    throw new EntityNotFoundException($"LoadingUnit ID={loadingUnitId}");
                }

                position.LoadingUnit = loadingUnit;
            }

            this.dataContext.SaveChanges();
        }

        public void UpdateHoming(BayNumber bayNumber, bool isExecuted)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Carousel)
                    .SingleOrDefault(b => b.Number == bayNumber);
                if (bay is null)
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }
                bay.Carousel.IsHomingExecuted = isExecuted;
                this.dataContext.SaveChanges();

                if (isExecuted)
                {
                    this.notificationEvent.Publish(
                        new NotificationMessage(
                            new BayOperationalStatusChangedMessageData
                            {
                                BayStatus = bay.Status,
                            },
                            $"Bay #{bay.Number} status changed to {bay.Status}",
                            MessageActor.MissionManager,
                            MessageActor.WebApi,
                            MessageType.BayOperationalStatusChanged,
                            bay.Number));
                }
            }
        }

        public void UpdateLastIdealPosition(double position, BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Carousel)
                    .SingleOrDefault(b => b.Number == bayNumber);
                if (bay is null)
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }

                bay.Carousel.LastIdealPosition = position;
                this.dataContext.SaveChanges();
            }
        }

        public Bay UpdatePosition(BayNumber bayNumber, int positionIndex, double height)
        {
            lock (this.dataContext)
            {
                var bay = this.GetByNumber(bayNumber);
                if (positionIndex < 1 || positionIndex > bay.Positions.Count())
                {
                    throw new ArgumentOutOfRangeException(Resources.Bays.TheSpecifiedBayPositionIsNotValid);
                }

                var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);
                if (height < verticalAxis.LowerBound || height > verticalAxis.UpperBound)
                {
                    throw new ArgumentOutOfRangeException(
                        string.Format(Resources.Bays.TheBayHeightMustBeInRange, height, verticalAxis.LowerBound, verticalAxis.UpperBound));
                }

                BayPosition position = null;

                if (positionIndex == 1)
                {
                    position = bay.Positions.Single(p => p.Height == bay.Positions.Max(e => e.Height));
                }

                if (positionIndex == 2)
                {
                    position = bay.Positions.Single(p => p.Height == bay.Positions.Min(e => e.Height));
                }

                position.Height = height;

                this.dataContext.SaveChanges();

                return this.GetByNumber(bayNumber);
            }
        }

        internal static string GetElevatorAxesCacheKey() => $"{nameof(GetElevatorAxes)}";

        private void Update(Bay bay)
        {
            if (bay is null)
            {
                throw new ArgumentNullException(nameof(bay));
            }

            lock (this.dataContext)
            {
                this.dataContext.Bays.Update(bay);

                this.dataContext.SaveChanges();
            }

            this.notificationEvent.Publish(
                new NotificationMessage(
                    new BayOperationalStatusChangedMessageData
                    {
                        BayStatus = bay.Status,
                    },
                    $"Bay #{bay.Number} status changed to {bay.Status}",
                    MessageActor.MissionManager,
                    MessageActor.WebApi,
                    MessageType.BayOperationalStatusChanged,
                    bay.Number));
        }

        #endregion
    }
}
