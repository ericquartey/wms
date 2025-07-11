using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.EntityFrameworkCore;
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

        private const string ROTATION_CLASS_A = "A";

        private const string ROTATION_CLASS_B = "B";

        private static readonly Func<DataLayerContext, IEnumerable<Bay>> GetAllCompile =
            EF.CompileQuery((DataLayerContext context) =>
            context.Bays
                    .AsNoTracking()
                    .Include(b => b.Inverter)
                    .Include(b => b.Accessories)
                        .ThenInclude(a => a.LaserPointer)
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
                EF.CompileQuery((DataLayerContext context, int bayNumber) =>
                context.Bays
                    .AsNoTracking()
                    .Include(b => b.Shutter)
                .SingleOrDefault(b => (int)b.Number == bayNumber));

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

        private static readonly Func<DataLayerContext, int, Bay> GetByLoadingUnitLocationCompile =
                EF.CompileQuery((DataLayerContext context, int number) =>
                context.Bays
                    .AsNoTracking()
                     .Include(b => b.Shutter)
                        .ThenInclude(i => i.Inverter)
                     .Include(b => b.Carousel)
                     .Include(b => b.External)
                     .Include(b => b.CurrentMission)
                    .FirstOrDefault(b => (int)b.Number == number));

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
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.Inverter)
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.AssistedMovements)
                    .Include(b => b.Shutter)
                        .ThenInclude(s => s.ManualMovements)
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

        private static readonly Func<DataLayerContext, int, Bay> GetPositionByLocationCompile =
                EF.CompileQuery((DataLayerContext context, int number) =>
                context.Bays
                .AsNoTracking()
                .Include(i => i.Carousel)
                .Include(i => i.External)
                .FirstOrDefault(b => (int)b.Number == number));

        private readonly IMemoryCache cache;

        private readonly MemoryCacheEntryOptions cacheOptions;

        private readonly DataLayerContext dataContext;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILogger<DataLayerContext> logger;

        private readonly IMachineProvider machineProvider;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly NotificationEvent notificationEvent;

        private readonly double profileConst0AGL = -212.5;

        private readonly double profileConst0ANG = -212.5;

        private readonly double profileConst0Factory = -212.5;

        private readonly double profileConst1AGL = 0.095;

        private readonly double profileConst1ANG = 0.0927;

        private readonly double profileConst1Factory = 0.0938;

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

                var machine = this.dataContext.Machines
                    .Include(m => m.Bays)
                    .First();

                machine.Bays.Add(new Bay
                {
                    Number = BayNumber.ElevatorBay,
                });

                this.dataContext.SaveChanges();
                this.machineVolatileDataProvider.BayNumbers = new List<BayNumber>();
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

        public void CheckBayFindZeroLimit()
        {
            lock (this.dataContext)
            {
                foreach (var bay in this.dataContext.Bays
                    .Include(b => b.Carousel)
                    .Where(b => b.Carousel != null))
                {
                    if (bay.Carousel.BayFindZeroLimit == 0)
                    {
                        bay.Carousel.BayFindZeroLimit = 6;
                        this.dataContext.SaveChanges();
                    }
                }
            }
        }

        public bool CheckIntrusion(BayNumber bayNumber, bool enable)
        {
            var bay = this.GetByNumberShutter(bayNumber);
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

        public void CheckProfileConst()
        {
            lock (this.dataContext)
            {
                foreach (var bay in this.dataContext.Bays.Where(b => b.Number < BayNumber.ElevatorBay))
                {
                    if (bay.ProfileConst1 == 0)
                    {
                        if (bay.Number == BayNumber.BayOne)
                        {
                            bay.ProfileConst1 = this.profileConst1ANG;
                            bay.ProfileConst0 = this.profileConst0ANG;
                        }
                        else
                        {
                            bay.ProfileConst1 = this.profileConst1AGL;
                            bay.ProfileConst0 = this.profileConst0AGL;
                        }
                        this.dataContext.SaveChanges();
                    }
                    if (string.IsNullOrEmpty(bay.RotationClass))
                    {
                        if (bay.Number == BayNumber.BayOne)
                        {
                            bay.RotationClass = ROTATION_CLASS_A;
                        }
                        else
                        {
                            bay.RotationClass = ROTATION_CLASS_B;
                        }
                        this.dataContext.SaveChanges();
                    }
                }
            }
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

        public double ConvertProfileToHeight(ushort profile, int positionId)
        {
            lock (this.dataContext)
            {
                var bay = this.GetByBayPositionId(positionId);
                if (bay is null)
                {
                    throw new EntityNotFoundException();
                }
                var heightMm = (profile * bay.ProfileConst1) + bay.ProfileConst0;
                var heightClass = (int)Math.Round(heightMm);
                heightClass = (heightClass / ProfileStep) * ProfileStep
                    + (((heightClass % ProfileStep) > 12) ? ProfileStep : 0)
                    + 24;
                var offset = bay.Positions.FirstOrDefault(x => x.Id == positionId)?.ProfileOffset ?? 0;
                this.logger.LogDebug($"positionId {positionId}; profile {profile}; height {heightMm + offset:0.00}; heightClass {heightClass}; k1 {bay.ProfileConst1}; k0 {bay.ProfileConst0}");
                return heightClass + offset;
            }
        }

        public void FindZero(BayNumber bayNumber)
        {
            this.PublishCommand(
                new HomingMessageData(Axis.BayChain, Calibration.FindSensor, null, true, false),
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
                var bays = GetAllCompile(this.dataContext).ToArray();
                foreach (var bay in bays)
                {
                    this.LoadBayPositions(bay);
                }
                return bays;
            }
        }

        public IEnumerable<BayNumber> GetBayNumbers()
        {
            if (!this.machineVolatileDataProvider.BayNumbers.Any())
            {
                lock (this.dataContext)
                {
                    this.machineVolatileDataProvider.BayNumbers = this.dataContext.Bays
                        .AsNoTracking()
                        .Select(b => b.Number)
                        .ToList();
                }
            }
            return this.machineVolatileDataProvider.BayNumbers;
        }

        public WarehouseSide GetBaySide(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Bays
                    .AsNoTracking()
                    .Select(b => new { b.Number, b.Side })
                    .First(b => b.Number == bayNumber)
                    .Side;
            }
        }

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
                var bayNumber = this.GetBayNumberByLocationId(bayPositionId);
                if (!bayNumber.HasValue)
                {
                    throw new EntityNotFoundException(bayPositionId);
                }
                var bay = GetByBayPositionIdCompile(this.dataContext, bayNumber.Value);
                if (bay is null)
                {
                    throw new EntityNotFoundException(bayPositionId);
                }

                this.LoadBayPositions(bay);
                return bay;
            }
        }

        public Bay GetByCell(Cell cell)
        {
            lock (this.dataContext)
            {
                var bay = GetByCellCompile(this.dataContext, cell);
                if (bay != null)
                {
                    foreach (var position in bay.Positions)
                    {
                        position.Bay = bay;
                    }
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
                        if (this.elevatorDataProvider.GetElevatorAxes().Any(a => a.Inverter?.Index == inverterIndex))
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
                    .Select(b => new { b.Number, b.IoDevice.Index })
                    .SingleOrDefault(b => b.Index == ioIndex);

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
                foreach (var position in bay.Positions)
                {
                    position.Bay = bay;
                }

                return bay;
            }
        }

        public Bay GetByLoadingUnitLocation(LoadingUnitLocation location)
        {
            lock (this.dataContext)
            {
                var bayNumber = this.GetBayNumberByLocation(location);
                if (!bayNumber.HasValue)
                {
                    return null;
                }
                var bay = GetByLoadingUnitLocationCompile(this.dataContext, bayNumber.Value);
                if (bay != null)
                {
                    this.LoadBayPositions(bay);
                }
                return bay;
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
                case MovementMode.ProfileResolution:
                case MovementMode.FindZero:
                case MovementMode.HorizontalResolution:
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
                this.LoadBayPositions(bay);

                return bay;
            }
        }

        public Bay GetByNumberCarousel(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                try
                {
                    var bay = this.dataContext.Bays
                        .AsNoTracking()
                        .Include(i => i.Carousel)
                            .ThenInclude(i => i.AssistedMovements)
                        .Include(i => i.Carousel)
                            .ThenInclude(i => i.ManualMovements)
                        .Include(b => b.FullLoadMovement)
                        .Include(b => b.EmptyLoadMovement)
                        .Where(b => b.Number == bayNumber)
                        .Single();

                    if (bay is null)
                    {
                        throw new EntityNotFoundException(bayNumber.ToString());
                    }
                    this.LoadBayPositions(bay);
                    return bay;
                }
                catch
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }
            }
        }

        public Bay GetByNumberExternal(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                try
                {
                    var bay = this.dataContext.Bays
                        .AsNoTracking()
                        .Include(i => i.External)
                            .ThenInclude(i => i.AssistedMovements)
                        .Include(i => i.External)
                            .ThenInclude(i => i.ManualMovements)
                        .Include(b => b.FullLoadMovement)
                        .Include(b => b.EmptyLoadMovement)
                        .Where(b => b.Number == bayNumber)
                        .Single();

                    if (bay is null)
                    {
                        throw new EntityNotFoundException(bayNumber.ToString());
                    }
                    this.LoadBayPositions(bay);
                    return bay;
                }
                catch
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }
            }
        }

        public Bay GetByNumberNoInclude(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                try
                {
                    var bay = this.dataContext.Bays
                        .AsNoTracking()
                        .Where(b => b.Number == bayNumber)
                        .Single();

                    if (bay is null)
                    {
                        throw new EntityNotFoundException(bayNumber.ToString());
                    }
                    return bay;
                }
                catch
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }
            }
        }

        public Bay GetByNumberPositions(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                try
                {
                    var bay = this.dataContext.Bays
                        .AsNoTracking()
                        .Where(b => b.Number == bayNumber)
                        .Single();

                    if (bay is null)
                    {
                        throw new EntityNotFoundException(bayNumber.ToString());
                    }
                    this.LoadBayPositions(bay);
                    return bay;
                }
                catch
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }
            }
        }

        public Bay GetByNumberShutter(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                try
                {
                    var bay = this.dataContext.Bays
                        .AsNoTracking()
                        .Include(i => i.Shutter)
                            .ThenInclude(s => s.Inverter)
                        .Include(b => b.Shutter)
                            .ThenInclude(s => s.AssistedMovements)
                        .Include(b => b.Shutter)
                            .ThenInclude(s => s.ManualMovements)
                        .Where(b => b.Number == bayNumber)
                        .Single();

                    if (bay is null)
                    {
                        throw new EntityNotFoundException(bayNumber.ToString());
                    }
                    return bay;
                }
                catch
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }
            }
        }

        public int GetCarouselBayFindZeroLimit(BayNumber bayNumber) => this.GetByNumberCarousel(bayNumber).Carousel.BayFindZeroLimit;

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

        public Inverter GetInverterByNumber(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                try
                {
                    var bay = this.dataContext.Bays
                        .AsNoTracking()
                        .Include(i => i.Inverter)
                        .Where(b => b.Number == bayNumber)
                        .Single();

                    if (bay is null || bay.Inverter is null)
                    {
                        throw new EntityNotFoundException(bayNumber.ToString());
                    }
                    return bay.Inverter;
                }
                catch
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }
            }
        }

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
                    returnValue = this.GetInverterByNumber(bayNumber).Index;
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
                        case MovementMode.ProfileResolution:
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

                        case MovementMode.FindZero:
                        case MovementMode.HorizontalCalibration:
                        case MovementMode.HorizontalResolution:
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
                            returnValue = this.GetByNumberShutter(bayNumber).Shutter.Inverter.Index;
                            break;

                        case MovementMode.BayChain:
                        case MovementMode.BayChainManual:
                        case MovementMode.BayTest:
                        case MovementMode.BayChainFindZero:
                        case MovementMode.DoubleExtBayTest:
                            returnValue = this.GetInverterByNumber(bayNumber).Index;
                            break;

                        case MovementMode.ExtBayChain:
                        case MovementMode.ExtBayChainManual:
                        case MovementMode.ExtBayTest:
                            returnValue = this.GetInverterByNumber(bayNumber).Index;
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
                    returnValue = this.GetByNumberShutter(bayNumber).Shutter.Inverter.Index;
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

        public bool GetIsExternal(BayNumber bayNumber)
        {
            bool isExternal = false;
            lock (this.dataContext)
            {
                if (!this.machineVolatileDataProvider.IsExternal.TryGetValue(bayNumber, out isExternal))
                {
                    isExternal = this.dataContext.Bays.AsNoTracking()
                        .Select(b => new { b.Number, b.IsExternal })
                        .SingleOrDefault(b => b.Number == bayNumber)
                        .IsExternal;
                    this.machineVolatileDataProvider.IsExternal.Add(bayNumber, isExternal);
                }
            }
            return isExternal;
        }

        public bool GetIsTelescopic(BayNumber bayNumber)
        {
            bool isTelescopic = false;
            lock (this.dataContext)
            {
                if (!this.machineVolatileDataProvider.IsTelescopic.TryGetValue(bayNumber, out isTelescopic))
                {
                    isTelescopic = this.dataContext.Bays.AsNoTracking()
                        .Select(b => new { b.Number, b.IsTelescopic })
                        .SingleOrDefault(b => b.Number == bayNumber)
                        .IsTelescopic;
                    this.machineVolatileDataProvider.IsTelescopic.Add(bayNumber, isTelescopic);
                }
            }
            return isTelescopic;
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
                    .Select(p => new { p.Location, p.LoadingUnit.Id })
                    .SingleOrDefault(p => p.Id == loadingUnitId)?.Location ?? LoadingUnitLocation.NoLocation;
            }
        }

        public BayPosition GetPositionById(int bayPositionId)
        {
            lock (this.dataContext)
            {
                var bayPosition = this.dataContext.BayPositions
                    .Include(b => b.LoadingUnit)
                    .AsNoTracking()
                    .Select(p => new BayPosition()
                    {
                        Id = p.Id,
                        BayId = p.BayId,
                        Height = p.Height,
                        IsBlocked = p.IsBlocked,
                        LoadingUnit = p.LoadingUnit,
                        Location = p.Location,
                        MaxDoubleHeight = p.MaxDoubleHeight,
                        MaxSingleHeight = p.MaxSingleHeight,
                        ProfileOffset = p.ProfileOffset
                    })
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
            lock (this.dataContext)
            {
                var bayNumber = this.GetBayNumberByLocation(location);
                if (!bayNumber.HasValue)
                {
                    throw new EntityNotFoundException(location.ToString());
                }
                var bay = GetPositionByLocationCompile(this.dataContext, bayNumber.Value);
                if (bay is null)
                {
                    throw new EntityNotFoundException(location.ToString());
                }
                this.LoadBayPositions(bay);

                return bay.Positions.FirstOrDefault(p => p.Location == location);
            }
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

        public InverterIndex GetShutterInverterIndex(BayNumber bayNumber)
        {
            var shutter = this.GetByNumberShutter(bayNumber).Shutter;
            if (shutter == null)
            {
                return InverterIndex.None;
            }

            return shutter.Inverter.Index;
        }

        public void IncrementCycles(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays.FirstOrDefault(b => b.Number == bayNumber);

                if (bay is null)
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }

                bay.TotalCycles++;
                this.dataContext.SaveChanges();
            }
        }

        public bool IsLoadUnitInBay(BayNumber bayNumber, int id)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Bays.Include(b => b.CurrentMission).Any(b => b.CurrentMission.LoadUnitId == id && b.Number == bayNumber);
            }
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

        public void NotifyRemoveLoadUnit(int loadingUnitId, LoadingUnitLocation location)
        {
            var messageData = new MoveLoadingUnitMessageData(
                        MissionType.NoType,
                        LoadingUnitLocation.NoLocation,
                        LoadingUnitLocation.NoLocation,
                        sourceCellId: null,
                        destinationCellId: null,
                        loadUnitId: loadingUnitId,
                        insertLoadUnit: false,
                        missionId: null,
                        loadUnitHeight: null,
                        netWeight: null,
                        CommonUtils.CommandAction.Start);

            BayNumber bayNumber;
            switch (location)
            {
                case LoadingUnitLocation.InternalBay1Up:
                case LoadingUnitLocation.InternalBay1Down:
                case LoadingUnitLocation.ExternalBay1Up:
                case LoadingUnitLocation.ExternalBay1Down:
                case LoadingUnitLocation.CarouselBay1Up:
                case LoadingUnitLocation.CarouselBay1Down:
                    bayNumber = BayNumber.BayOne;
                    break;

                case LoadingUnitLocation.InternalBay2Up:
                case LoadingUnitLocation.InternalBay2Down:
                case LoadingUnitLocation.ExternalBay2Up:
                case LoadingUnitLocation.ExternalBay2Down:
                case LoadingUnitLocation.CarouselBay2Up:
                case LoadingUnitLocation.CarouselBay2Down:
                    bayNumber = BayNumber.BayTwo;
                    break;

                case LoadingUnitLocation.InternalBay3Up:
                case LoadingUnitLocation.InternalBay3Down:
                case LoadingUnitLocation.ExternalBay3Up:
                case LoadingUnitLocation.ExternalBay3Down:
                case LoadingUnitLocation.CarouselBay3Up:
                case LoadingUnitLocation.CarouselBay3Down:
                    bayNumber = BayNumber.BayThree;
                    break;

                default:
                    bayNumber = BayNumber.None;
                    break;
            }
            this.notificationEvent.Publish(
                    new NotificationMessage
                    {
                        Data = messageData,
                        Destination = MessageActor.Any,
                        Source = MessageActor.WebApi,
                        Type = MessageType.RemoveLoadUnit,
                        RequestingBay = bayNumber,
                    });
        }

        public void PerformHoming(BayNumber bayNumber)
        {
            this.PublishCommand(
                new HomingMessageData(Axis.BayChain, Calibration.ResetEncoder, null, true, false),
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

                this.NotifyRemoveLoadUnit(loadingUnitId, sourceBay);
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
                    if (bay.CurrentMission != null)
                    {
                        bay.CurrentMission = null;
                        this.Update(bay);
                    }
                }
            }
        }

        public void SetAllOperationsBay(bool pick, bool put, bool view, bool inventory, bool barcodeAutomaticPut, int bayid, bool showBarcodeImage, bool checkListContinueInOtherMachine, bool IsNrLabelsEditable)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays.SingleOrDefault(b => b.Id == bayid);
                bay.Pick = pick;
                bay.Put = put;
                bay.View = view;
                bay.Inventory = inventory;
                bay.BarcodeAutomaticPut = barcodeAutomaticPut;
                bay.ShowBarcodeImage = showBarcodeImage;
                bay.CheckListContinueInOtherMachine = checkListContinueInOtherMachine;
                bay.IsNrLabelEditable = IsNrLabelsEditable;
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
                var bay = this.dataContext.Bays.FirstOrDefault(b => b.Number == targetBay);
                if (bay is null)
                {
                    throw new EntityNotFoundException(targetBay.ToString());
                }

                bay.Operation = newOperation;

                this.dataContext.SaveChanges();

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

        public void SetProfileConstBay(BayNumber bayNumber, double k0, double k1)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays.SingleOrDefault(b => b.Number == bayNumber);
                bay.ProfileConst0 = k0;
                bay.ProfileConst1 = k1;

                this.dataContext.SaveChanges();
            }
        }

        public void SetRotationClass(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                foreach (var bay in this.dataContext.Bays
                    .Where(b => b.Number < BayNumber.ElevatorBay))
                {
                    if (bay.Number == bayNumber)
                    {
                        bay.RotationClass = ROTATION_CLASS_A;
                    }
                    else
                    {
                        bay.RotationClass = ROTATION_CLASS_B;
                    }
                }

                this.dataContext.SaveChanges();
            }
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

        public void UpdateELevatorDistance(BayNumber bayNumber, double distance)
        {
            lock (this.dataContext)
            {
                var bay = this.GetByNumberCarousel(bayNumber);
                if (bay.Carousel != null)
                {
                    bay.Carousel.ElevatorDistance = distance;
                    this.dataContext.AddOrUpdate(bay.Carousel, f => f.Id);
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void UpdateExtraRace(BayNumber bayNumber, double extraRace)
        {
            lock (this.dataContext)
            {
                var bay = this.GetByNumberExternal(bayNumber);
                if (bay.External != null)
                {
                    bay.External.ExtraRace = extraRace;
                    this.dataContext.AddOrUpdate(bay.External, f => f.Id);
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void UpdateLastCalibrationCycles(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays.FirstOrDefault(b => b.Number == bayNumber);

                if (bay is null)
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }

                bay.LastCalibrationCycles = bay.TotalCycles;
                this.dataContext.SaveChanges();
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
                if (!this.GetIsExternal(bayNumber))
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
                else
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
                var bay = this.GetByNumberPositions(bayNumber);
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
                var bay = this.GetByNumberExternal(bayNumber);
                if (bay.External != null)
                {
                    bay.External.Race = race;
                    this.dataContext.AddOrUpdate(bay.External, f => f.Id);
                    this.dataContext.SaveChanges();
                }
            }
        }

        public void UpdateResolution(BayNumber bayNumber, double newRace)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.External)
                    .SingleOrDefault(b => b.Number == bayNumber);
                if (bay.External != null)
                {
                    bay.Resolution = bay.Resolution * bay.External.Race / newRace;
                    this.dataContext.SaveChanges();
                }
            }
        }

        internal static string GetInverterIndexCacheKey(InverterIndex inverterIndex) => $"{nameof(GetByInverterIndex)}{inverterIndex}";

        private int? GetBayNumberByLocation(LoadingUnitLocation location)
        {
            return this.dataContext.BayPositions.AsNoTracking()
                                                .Where(p => p.Location == location)
                                                .Select(p => p.BayId)
                                                .SingleOrDefault();
        }

        private int? GetBayNumberByLocationId(int locationId)
        {
            return this.dataContext.BayPositions.AsNoTracking()
                                                .Where(p => p.Id == locationId)
                                                .Select(p => p.BayId)
                                                .SingleOrDefault();
        }

        /// <summary>
        /// TODO, this method it's dublicated because the insert in LoadUnitDataProvider generate a circural ref error
        /// </summary>
        /// <param name="loadingUnitsId"></param>
        /// <returns></returns>
        private LoadingUnit InsertLoadingUnit(int loadingUnitsId)
        {
            this.logger.LogWarning("InsertLoadingUnit, loading unit is null, try do instance a newer");
            var loadingUnits = new LoadingUnit();

            var machine = this.machineProvider.GetMinMaxHeight();
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

        private void LoadBayPositions(Bay bay)
        {
            var positions = this.dataContext.BayPositions
                .Include(i => i.LoadingUnit)
                .Select(p => new BayPosition()
                {
                    Id = p.Id,
                    BayId = p.BayId,
                    Height = p.Height,
                    IsBlocked = p.IsBlocked,
                    LoadingUnit = p.LoadingUnit,
                    Location = p.Location,
                    MaxDoubleHeight = p.MaxDoubleHeight,
                    MaxSingleHeight = p.MaxSingleHeight,
                    ProfileOffset = p.ProfileOffset
                })
                .Where(p => p.BayId == bay.Id)
                .ToList();
            foreach (var position in positions)
            {
                position.Bay = bay;
            }
            bay.Positions = positions;
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
