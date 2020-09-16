using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class BaysDataProvider : BaseProvider, IBaysDataProvider
    {
        //private const double AdditionalStorageSpace = 14.5;     // AdditionalStorageSpace + VerticalPositionTolerance = 27mm

        #region Fields

        private const int ProfileStep = 25;

        private static readonly Func<DataLayerContext, IEnumerable<Bay>> GetAllCompile =
            EF.CompileQuery((DataLayerContext context) =>
            context.Bays
                    .AsNoTracking()
                    .Include(b => b.Inverter)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.LaserPointer)
                    .Include(b => b.Positions)
                        .ThenInclude(s => s.LoadingUnit)
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.AssistedMovements)
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.ManualMovements)
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.Inverter)
                    .Include(b => b.Carousel)
                    .Include(b => b.External)
                    .Include(b => b.EmptyLoadMovement)
                    .Include(b => b.FullLoadMovement)
                    .Include(b => b.CurrentMission));

        private static readonly Func<DataLayerContext, int, Bay> GetByBayPositionIdCompile =
                EF.CompileQuery((DataLayerContext context, int bayPositionId) =>
                context.Bays
                    .AsNoTracking()
                    .Include(b => b.Positions)
                        .ThenInclude(s => s.LoadingUnit)
                    .Include(b => b.Shutter)
                .SingleOrDefault(b => b.Positions.Any(p => p.Id == bayPositionId)));

        private static readonly Func<DataLayerContext, Cell, Bay> GetByCellCompile =
                EF.CompileQuery((DataLayerContext context, Cell cell) =>
                context.Bays
                    .AsNoTracking()
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.Inverter)
                    .Include(b => b.Carousel)
                    .Include(b => b.External)
                    .Include(b => b.Positions)
                    .Where(b => b.Side == cell.Side && b.Positions.First().Height < cell.Position)
                    .OrderBy(o => cell.Position - o.Positions.First().Height)
                    .FirstOrDefault());

        private static readonly Func<DataLayerContext, IoIndex, Bay> GetByIoIndexCompile =
                EF.CompileQuery((DataLayerContext context, IoIndex ioIndex) =>
                context.Bays
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
                    .Include(b => b.External)
                    .Include(b => b.EmptyLoadMovement)
                    .Include(b => b.FullLoadMovement)
                    .SingleOrDefault(b => b.IoDevice.Index == ioIndex));

        private static readonly Func<DataLayerContext, LoadingUnitLocation, Bay> GetByLoadingUnitLocationCompile =
                EF.CompileQuery((DataLayerContext context, LoadingUnitLocation location) =>
                context.Bays
                    .AsNoTracking()
                     .Include(b => b.Shutter)
                        .ThenInclude(i => i.Inverter)
                     .Include(b => b.Carousel)
                     .Include(b => b.External)
                     .Include(b => b.Positions)
                        .ThenInclude(t => t.LoadingUnit)
                     .Include(b => b.CurrentMission)
                    .FirstOrDefault(b => b.Positions.Any(p => p.Location == location)));

        private static readonly Func<DataLayerContext, BayNumber, Bay> GetByNumberCompile =
                                EF.CompileQuery((DataLayerContext context, BayNumber bayNumber) =>
                context.Bays
                    .AsNoTracking()
                    .Include(b => b.Inverter)
                    .Include(b => b.CurrentMission)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.AlphaNumericBar)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.BarcodeReader)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.CardReader)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.LabelPrinter)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.LaserPointer)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.TokenReader)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.WeightingScale)
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
                    .Include(b => b.External)
                        .ThenInclude(s => s.ManualMovements)
                    .Include(b => b.External)
                        .ThenInclude(s => s.AssistedMovements)
                    .Include(b => b.EmptyLoadMovement)
                    .Include(b => b.FullLoadMovement)
                    .SingleOrDefault(b => b.Number == bayNumber));

        private static readonly Func<DataLayerContext, LoadingUnitLocation, BayPosition> GetPositionByLocationCompile =
                EF.CompileQuery((DataLayerContext context, LoadingUnitLocation location) =>
                context.BayPositions
                    .AsNoTracking()
                .Include(b => b.Bay)
                    .ThenInclude(i => i.Carousel)
                .Include(b => b.Bay)
                    .ThenInclude(i => i.External)
                .Include(b => b.LoadingUnit)
                .SingleOrDefault(p => p.Location == location));

        private readonly IMemoryCache cache;

        private readonly MemoryCacheEntryOptions cacheOptions;

        private readonly DataLayerContext dataContext;

        private readonly IElevatorDataProvider elevatorDataProvider;

        /// <summary>
        /// TODO move to configuration
        /// </summary>
        private readonly double kMul = 0.090625;

        private readonly double kMulNew = 0.0938;

        /// <summary>
        /// TODO move to configuration
        /// </summary>
        private readonly double kSum = -181.25;

        private readonly double kSumNew = -212.5;

        private readonly ILogger<DataLayerContext> logger;

        private readonly IMachineProvider machineProvider;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly NotificationEvent notificationEvent;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public BaysDataProvider(
            DataLayerContext dataContext,
            IEventAggregator eventAggregator,
            IMachineProvider machineProvider,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            IConfiguration configuration,
            IElevatorDataProvider elevatorDataProvider,
            IMemoryCache memoryCache,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ILogger<DataLayerContext> logger)
            : base(eventAggregator)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));

            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
            this.cacheOptions = configuration.GetMemoryCacheOptions();
        }

        #endregion

        #region Methods

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

                bay.CurrentMission = this.dataContext.Missions.SingleOrDefault(m => m.Id == mission.Id);
                if (bay.CurrentMission is null)
                {
                    throw new EntityNotFoundException(mission.Id);
                }

                this.Update(bay);

                return bay;
            }
        }

        public bool CheckIntrusion(BayNumber bayNumber, bool enable)
        {
            var bay = this.GetByNumber(bayNumber);
            if (bay.IsCheckIntrusion
                && (bay.Shutter is null || bay.Shutter.Type == ShutterType.NotSpecified)
                )
            {
                this.PublishCommand(
                    new CheckIntrusionMessageData(enable),
                    "Execute Check Intrusion Command",
                    MessageActor.DeviceManager,
                    MessageType.CheckIntrusion,
                    bayNumber,
                    bayNumber);
                return true;
            }
            return false;
        }

        public Bay ClearMission(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays.Include(b => b.CurrentMission).SingleOrDefault(b => b.Number == bayNumber);
                if (bay is null)
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }

                bay.CurrentMission = null;

                this.Update(bay);

                return bay;
            }
        }

        /// <summary>
        /// <param name="profile">value read from inverter</param>
        /// profile = 2000  ==> height = 0
        /// profile = 10000 ==> height = 725mm
        /// height = kMul * profile + kSum;
        /// </summary>
        [Obsolete("This method contains business logic. It should not be in the DataLayer.")]
        public double ConvertProfileToHeight(ushort profile, int positionId)
        {
            lock (this.dataContext)
            {
                var bay = this.GetByBayPositionId(positionId);
                if (bay is null)
                {
                    throw new EntityNotFoundException();
                }
                var offset = bay.Positions.FirstOrDefault(x => x.Id == positionId)?.ProfileOffset ?? 0;
                return (profile * this.kMul) + this.kSum + offset;
            }
        }

        public double ConvertProfileToHeightNew(ushort profile, int positionId)
        {
            lock (this.dataContext)
            {
                var bay = this.GetByBayPositionId(positionId);
                if (bay is null)
                {
                    throw new EntityNotFoundException();
                }
                var heightMm = (profile * this.kMulNew) + this.kSumNew;
                var heightClass = (int)Math.Round(heightMm);
                heightClass = (heightClass / ProfileStep) * ProfileStep
                    + (((heightClass % ProfileStep) > 12) ? ProfileStep : 0)
                    + 24;
                var offset = bay.Positions.FirstOrDefault(x => x.Id == positionId)?.ProfileOffset ?? 0;
                //this.logger.LogDebug($"positionId {positionId}; profile {profile}; height {heightMm + offset}; heightClass {heightClass}");
                return heightClass + offset;
            }
        }

        public void FindZero(BayNumber bayNumber)
        {
            this.PublishCommand(
                new HomingMessageData(Axis.BayChain, Calibration.FindSensor, null, true),
                "Execute FindZeroSensor Command",
                MessageActor.DeviceManager,
                MessageType.Homing,
                bayNumber,
                bayNumber);
        }

        public BayAccessories GetAccessories(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.AlphaNumericBar)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.BarcodeReader)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.CardReader)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.LabelPrinter)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.LaserPointer)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.TokenReader)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.WeightingScale)
                    .AsNoTracking()
                    .SingleOrDefault(b => b.Number == bayNumber);

                return bay.Accessories ?? new BayAccessories();
            }
        }

        public IEnumerable<Bay> GetAll()
        {
            lock (this.dataContext)
            {
                return GetAllCompile(this.dataContext).ToArray();
            }
        }

        public int GetAllCount()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Bays.AsNoTracking().Count();
            }
        }

        public CarouselManualParameters GetAssistedMovementsCarousel(BayNumber bayNumber) => this.GetByNumber(bayNumber).Carousel.AssistedMovements;

        public ExternalBayManualParameters GetAssistedMovementsExternalBay(BayNumber bayNumber) => this.GetByNumber(bayNumber).External.AssistedMovements;

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
                var bay = GetByBayPositionIdCompile(this.dataContext, bayPositionId);
                if (bay is null)
                {
                    throw new EntityNotFoundException(bayPositionId);
                }

                return bay;
            }
        }

        public Bay GetByCell(Cell cell)
        {
            lock (this.dataContext)
            {
                return GetByCellCompile(this.dataContext, cell);
            }
        }

        public Bay GetByIdOrDefault(int id)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Bays.AsNoTracking().SingleOrDefault(b => b.Id == id);
            }
        }

        public BayNumber GetByInverterIndex(InverterIndex inverterIndex, FieldMessageType messageType = FieldMessageType.NoType)
        {
            lock (this.dataContext)
            {
                if (messageType == FieldMessageType.MeasureProfile && inverterIndex == InverterIndex.MainInverter)
                {
                    return BayNumber.BayOne;
                }
                var cacheKey = GetInverterIndexCacheKey(inverterIndex);
                if (!this.cache.TryGetValue(cacheKey, out BayNumber cacheEntry))
                {
                    var bay = this.dataContext.Bays
                        .Include(i => i.Inverter)
                        .Include(i => i.Shutter)
                            .ThenInclude(s => s.Inverter)
                        .AsNoTracking()
                        .SingleOrDefault(b =>
                            b.Inverter.Index == inverterIndex
                            || b.Shutter.Inverter.Index == inverterIndex);

                    if (bay is null)
                    {
                        if (this.elevatorDataProvider.GetElevatorAxes().Any(a => a.Inverter.Index == inverterIndex))
                        {
                            cacheEntry = BayNumber.ElevatorBay;
                        }
                        else
                        {
                            throw new EntityNotFoundException(inverterIndex.ToString());
                        }
                    }
                    else
                    {
                        cacheEntry = bay.Number;
                    }

                    this.cache.Set(cacheKey, cacheEntry, this.cacheOptions);
                }
                return cacheEntry;
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
                var bay = GetByIoIndexCompile(this.dataContext, ioIndex);

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
                return GetByLoadingUnitLocationCompile(this.dataContext, location);
            }
        }

        [Obsolete("This method contains business logic. It should not be in the DataLayer.")]
        public BayNumber GetByMovementType(IPositioningMessageData data)
        {
            BayNumber targetBay;
            switch (data.MovementMode)
            {
                case MovementMode.BeltBurnishing:
                case MovementMode.HorizontalCalibration:
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
                var bay = GetByNumberCompile(this.dataContext, bayNumber);

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
            return this.machineVolatileDataProvider.GetBayEncoderPosition(bayNumber);
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
                                    returnValue = this.machineVolatileDataProvider.IsOneTonMachine.Value ? InverterIndex.Slave1 : InverterIndex.MainInverter;
                                    break;

                                case Axis.Vertical:
                                    returnValue = InverterIndex.MainInverter;
                                    break;

                                default:
                                    throw new InvalidOperationException(Resources.Bays.ResourceManager.GetString("TheBayHorizontalAndVerticalNotValid", CommonUtils.Culture.Actual));
                            }

                            break;

                        case MovementMode.BeltBurnishing:
                        case MovementMode.TorqueCurrentSampling:
                            returnValue = InverterIndex.MainInverter;
                            break;

                        case MovementMode.HorizontalCalibration:
                            returnValue = this.machineVolatileDataProvider.IsOneTonMachine.Value ? InverterIndex.Slave1 : InverterIndex.MainInverter;
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

                        case MovementMode.ExtBayChain:
                        case MovementMode.ExtBayChainManual:
                        case MovementMode.ExtBayTest:
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

        public bool GetLightOn(BayNumber bayNumber)
        {
            return this.machineVolatileDataProvider.IsBayLightOn.GetValueOrDefault(bayNumber);
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

        public ExternalBayManualParameters GetManualMovementsExternalBay(BayNumber bayNumber) => this.GetByNumber(bayNumber).External.ManualMovements;

        public ShutterManualParameters GetManualMovementsShutter(BayNumber bayNumber) => this.GetByNumber(bayNumber).Shutter.ManualMovements;

        public BayPosition GetPositionById(int bayPositionId)
        {
            lock (this.dataContext)
            {
                var bayPosition = this.dataContext.BayPositions
                    .Include(b => b.LoadingUnit)
                    .AsNoTracking()
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
            var bayPosition = GetPositionByLocationCompile(this.dataContext, location);
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

        //public InverterIndex GetShutterInverterIndex(BayNumber bayNumber) => this.GetByNumber(bayNumber).Shutter.Inverter.Index;
        public InverterIndex GetShutterInverterIndex(BayNumber bayNumber)
        {
            var shutter = this.GetByNumber(bayNumber).Shutter;
            if (shutter == null)
            {
                return InverterIndex.None;
            }

            return shutter.Inverter.Index;
        }

        public bool IsMissionInBay(Mission mission)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Bays.Include(b => b.CurrentMission).Any(b => b.CurrentMission.Id == mission.Id);
            }
        }

        public void Light(BayNumber bayNumber, bool enable)
        {
            this.PublishCommand(
                new BayLightMessageData(enable),
                "Execute BayLigth Command",
                MessageActor.DeviceManager,
                MessageType.BayLight,
                bayNumber,
                bayNumber);
        }

        public void PerformHoming(BayNumber bayNumber)
        {
            this.PublishCommand(
                new HomingMessageData(Axis.BayChain, Calibration.ResetEncoder, null, true),
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

            var sourceBay = this.GetLoadingUnitLocationByLoadingUnit(loadingUnitId);
            if (sourceBay != LoadingUnitLocation.NoLocation)
            {
                var positionId = this.GetPositionByLocation(sourceBay).Id;
                this.SetLoadingUnit(positionId, null);
            }

            lock (this.dataContext)
            {
                this.dataContext.LoadingUnits.Remove(lu);
                this.dataContext.SaveChanges();
            }
        }

        public void ResetMachine()
        {
            lock (this.dataContext)
            {
                foreach (var bayPosition in this.dataContext.BayPositions.Include(i => i.LoadingUnit))
                {
                    if (bayPosition.LoadingUnit != null)
                    {
                        var lu = this.dataContext.LoadingUnits.SingleOrDefault(l => l.Id == bayPosition.LoadingUnit.Id);
                        if (lu != null)
                        {
                            lu.Status = DataModels.Enumerations.LoadingUnitStatus.Undefined;
                            this.dataContext.LoadingUnits.Update(lu);
                        }

                        bayPosition.LoadingUnit = null;
                        this.dataContext.BayPositions.Update(bayPosition);
                    }
                }

                foreach (var bay in this.dataContext.Bays.Include(i => i.CurrentMission))
                {
                    bay.CurrentMission = null;
                    this.Update(bay);
                }
            }
        }

        public void SetAllOpertionBay(bool pick, bool put, bool view, bool inventory, int bayid)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays.AsNoTracking().SingleOrDefault(b => b.Id == bayid);
                bay.Pick = pick;
                bay.Put = put;
                bay.View = view;
                bay.Inventory = inventory;

                this.dataContext.Bays.Update(bay);
                this.dataContext.SaveChanges();
            }
        }

        public void SetAlphaNumericBar(BayNumber bayNumber, bool isEnabled, string ipAddress, int port)
        {
            lock (this.dataContext)
            {
                var barBay = this.dataContext.Bays.Include(b => b.Accessories)
                        .ThenInclude(a => a.AlphaNumericBar)
                        .Single(b => b.Number == bayNumber);

                barBay.Accessories.AlphaNumericBar.IsEnabledNew = isEnabled;
                barBay.Accessories.AlphaNumericBar.IpAddress = IPAddress.Parse(ipAddress);
                barBay.Accessories.AlphaNumericBar.TcpPort = port;

                this.dataContext.Accessories.Update(barBay.Accessories.AlphaNumericBar);
                this.dataContext.SaveChanges();
            }
        }

        public Bay SetBayActive(BayNumber bayNumber, bool active)
        {
            // TODO: Check bay activation logic

            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays.FirstOrDefault(b => b.Number == bayNumber);

                if (bay is null)
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }

                bay.IsActive = active;
                this.dataContext.SaveChanges();

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

                this.logger.LogInformation($"The bay {bay.Number} is now {(active ? "active" : "deactivated")}");
                return bay;
            }
        }

        public void SetChainPosition(BayNumber bayNumber, double value)
        {
            this.machineVolatileDataProvider.SetBayEncoderPosition(bayNumber, value);
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

        public void SetLaserPointer(BayNumber bayNumber, bool isEnabled, string ipAddress, int port, double xOffset, double yOffset, double zOffsetLowerPosition, double zOffsetUpperPosition)
        {
            lock (this.dataContext)
            {
                var laserPointerBay = this.dataContext.Bays.Include(b => b.Accessories)
                        .ThenInclude(a => a.LaserPointer)
                        .Single(b => b.Number == bayNumber);

                var laserPointer = laserPointerBay.Accessories.LaserPointer;

                laserPointer.IsEnabledNew = isEnabled;
                laserPointer.IpAddress = IPAddress.Parse(ipAddress);
                laserPointer.TcpPort = port;
                laserPointer.XOffset = xOffset;
                laserPointer.YOffset = yOffset;
                laserPointer.ZOffsetLowerPosition = zOffsetLowerPosition;
                laserPointer.ZOffsetUpperPosition = zOffsetUpperPosition;

                this.dataContext.Accessories.Update(laserPointer);
                this.dataContext.SaveChanges();
            }
        }

        public void SetLoadingUnit(int bayPositionId, int? loadingUnitId, double? height = null)
        {
            var position = this.dataContext.BayPositions.Include(i => i.LoadingUnit).SingleOrDefault(p => p.Id == bayPositionId);
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
                    loadingUnit = this.InsertLoadingUnit(loadingUnitId.Value);  //TODO: must be fixed
                    //throw new EntityNotFoundException($"LoadingUnit ID={loadingUnitId}");
                }

                loadingUnit.Status = DataModels.Enumerations.LoadingUnitStatus.InBay;
                if (height.HasValue)
                {
                    loadingUnit.Height = height.Value;
                }
                position.LoadingUnit = loadingUnit;

                this.dataContext.LoadingUnits.Update(loadingUnit);
            }

            this.dataContext.BayPositions.Update(position);
            this.dataContext.SaveChanges();
        }

        public void UpdateBarcodeReaderSettings(BayNumber bayNumber, bool isEnabled, string portName)
        {
            if (portName is null)
            {
                throw new ArgumentNullException(nameof(portName));
            }

            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Accessories)
                    .ThenInclude(a => a.BarcodeReader)
                    .Single(b => b.Number == bayNumber);

                bay.Accessories.BarcodeReader.IsEnabledNew = isEnabled;
                bay.Accessories.BarcodeReader.PortName = portName;

                this.dataContext.Accessories.Update(bay.Accessories.BarcodeReader);
                this.dataContext.SaveChanges();
            }
        }

        public void UpdateCardReaderSettings(BayNumber bayNumber, bool isEnabled, string tokenRegex)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Accessories)
                    .ThenInclude(a => a.CardReader)
                    .Single(b => b.Number == bayNumber);

                bay.Accessories.CardReader.IsEnabledNew = isEnabled;
                bay.Accessories.CardReader.TokenRegex = tokenRegex;

                this.dataContext.Accessories.Update(bay.Accessories.CardReader);
                this.dataContext.SaveChanges();
            }
        }

        public void UpdateELevatorDistance(BayNumber bayNumber, double distance)
        {
            lock (this.dataContext)
            {
                var bay = this.GetByNumber(bayNumber);
                if (bay.Carousel != null)
                {
                    bay.Carousel.ElevatorDistance = distance;
                    this.dataContext.AddOrUpdate(bay.Carousel, f => f.Id);
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void UpdateLastIdealPosition(double position, BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                //var bay = this.dataContext.Bays
                //    .Include(b => b.Carousel)
                //    .SingleOrDefault(b => b.Number == bayNumber);
                //if (bay is null)
                //{
                //    throw new EntityNotFoundException(bayNumber.ToString());
                //}

                //bay.Carousel.LastIdealPosition = position;
                //this.dataContext.SaveChanges();

                // Retrieve type of bay
                var currBay = this.GetByNumber(bayNumber);

                if (currBay.Carousel != null)
                {
                    // Handle the carousel
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

                if (currBay.External != null)
                {
                    // Handle the external bay
                    var bay = this.dataContext.Bays
                        .Include(b => b.External)
                        .SingleOrDefault(b => b.Number == bayNumber);
                    if (bay is null)
                    {
                        throw new EntityNotFoundException(bayNumber.ToString());
                    }

                    bay.External.LastIdealPosition = position;
                    this.dataContext.SaveChanges();
                }
            }
        }

        public Bay UpdatePosition(BayNumber bayNumber, int positionIndex, double height)
        {
            lock (this.dataContext)
            {
                var bay = this.GetByNumber(bayNumber);
                if (positionIndex < 1 || positionIndex > bay.Positions.Count())
                {
                    throw new ArgumentOutOfRangeException(Resources.Bays.ResourceManager.GetString("TheSpecifiedBayPositionIsNotValid", CommonUtils.Culture.Actual));
                }

                var verticalAxis = this.elevatorDataProvider.GetAxis(Orientation.Vertical);
                if (height < verticalAxis.LowerBound || height > verticalAxis.UpperBound)
                {
                    throw new ArgumentOutOfRangeException(
                        string.Format(Resources.Bays.ResourceManager.GetString("TheBayHeightMustBeInRange", CommonUtils.Culture.Actual), height, verticalAxis.LowerBound, verticalAxis.UpperBound));
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

                var procedureParameters = this.setupProceduresDataProvider.GetBayHeightCheck(bayNumber);
                this.setupProceduresDataProvider.MarkAsCompleted(procedureParameters);

                this.dataContext.BayPositions.Update(position);
                this.dataContext.SaveChanges();

                return this.GetByNumber(bayNumber);
            }
        }

        public void UpdateRace(BayNumber bayNumber, double race)
        {
            lock (this.dataContext)
            {
                var bay = this.GetByNumber(bayNumber);
                if (bay.External != null)
                {
                    bay.External.Race = race;
                    this.dataContext.AddOrUpdate(bay.External, f => f.Id);
                    this.dataContext.SaveChanges();
                }
            }
        }

        internal static string GetInverterIndexCacheKey(InverterIndex inverterIndex) => $"{nameof(GetByInverterIndex)}{inverterIndex}";

        /// <summary>
        /// TODO, this method it's dublicated because the insert in LoadUnitDataProvider generate a circural ref error
        /// </summary>
        /// <param name="loadingUnitsId"></param>
        /// <returns></returns>
        private LoadingUnit InsertLoadingUnit(int loadingUnitsId)
        {
            this.logger.LogWarning("InsertLoadingUnit, loading unit is null, try do instance a newer");
            var loadingUnits = new LoadingUnit();

            var machine = this.machineProvider.Get();
            lock (this.dataContext)
            {
                loadingUnits = new LoadingUnit
                {
                    Id = loadingUnitsId,
                    Tare = machine.LoadUnitTare,
                    MaxNetWeight = machine.LoadUnitMaxNetWeight,
                    Height = 0
                };

                this.dataContext.LoadingUnits.Add(loadingUnits);

                this.dataContext.SaveChanges();
            }

            return loadingUnits;
        }

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
        }

        #endregion
    }
}
