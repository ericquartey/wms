using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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

        internal static string GetBayCellCacheKey = "BayCellKey";

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

        // this query does not work in EF 3.1
        //private static readonly Func<DataLayerContext, Cell, Bay> GetByCellCompile =
        //        EF.CompileQuery((DataLayerContext context, Cell cell) =>
        //        context.Bays
        //            .AsNoTracking()
        //            .Include(b => b.Shutter)
        //                .ThenInclude(s => s.Inverter)
        //            .Include(b => b.Carousel)
        //            .Include(b => b.External)
        //            .Include(b => b.Positions)
        //            .Where(b => b.Side == cell.Side && b.Positions.First().Height < cell.Position)
        //            .OrderBy(o => cell.Position - o.Positions.First().Height)
        //            .FirstOrDefault());

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
                        .ThenInclude(s => s.ManualMovements)
                     .Include(b => b.Carousel)
                        .ThenInclude(s => s.AssistedMovements)
                     .Include(b => b.External)
                     .Include(b => b.Positions)
                        .ThenInclude(t => t.LoadingUnit)
                     .Include(b => b.CurrentMission)
                    .Include(b => b.EmptyLoadMovement)
                    .Include(b => b.FullLoadMovement)
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

        //private static int cacheHit;

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

        #region Properties

        internal static string GetBayAllCacheKey => "BayAllKey";

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

                this.RemoveCache(BayNumber.ElevatorBay);
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
                        this.RemoveCache(bay.Number);
                    }
                }
            }
        }

        public bool CheckIntrusion(BayNumber bayNumber, bool enable)
        {
            lock (this.dataContext)
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
                        this.RemoveCache(bay.Number);
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
                        this.RemoveCache(bay.Number);
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
                this.cache.TryGetValue(GetBayAllCacheKey, out IEnumerable<Bay> cacheEntry);
                if (cacheEntry is null)
                {
                    var bays = GetAllCompile(this.dataContext).ToArray();

                    if (bays is null)
                    {
                        throw new EntityNotFoundException("BAYS");
                    }
                    this.cache.Set(GetBayAllCacheKey, bays, this.cacheOptions);
                    cacheEntry = bays;
                }
                //else
                //{
                //    cacheHit++;
                //}

                return cacheEntry;
            }
        }

        public IEnumerable<BayNumber> GetBayNumbers()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Bays
                    .AsNoTracking()
                    .Select(b => b.Number);
            }
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
                this.cache.TryGetValue(GetBayPositionCacheKey(bayPositionId), out Bay cacheEntry);
                if (cacheEntry is null)
                {
                    var bay = GetByBayPositionIdCompile(this.dataContext, bayPositionId);

                    if (bay is null)
                    {
                        throw new EntityNotFoundException(bayPositionId.ToString());
                    }
                    this.cache.Set(GetBayPositionCacheKey(bayPositionId), bay, this.cacheOptions);
                    cacheEntry = bay;
                }
                //else
                //{
                //    cacheHit++;
                //}

                return cacheEntry;
            }
        }

        public Bay GetByCell(Cell cell)
        {
            lock (this.dataContext)
            {
                var cacheKey = this.cache.TryGetValue(GetBayCellCacheKey, out Bay cacheEntry);
                if (cacheEntry is null)
                {
                    var bay = this.dataContext.Bays
                        .AsNoTracking()
                        .Include(b => b.Shutter)
                            .ThenInclude(s => s.Inverter)
                        .Include(b => b.Carousel)
                        .Include(b => b.External)
                        .Include(b => b.Positions)
                        .AsEnumerable()
                        .Where(b => b.Side == cell.Side && b.Positions.All(p => p.Height < cell.Position))
                        .OrderBy(o => cell.Position - o.Positions.First().Height)
                        .FirstOrDefault();

                    this.cache.Set(GetBayCellCacheKey, bay, this.cacheOptions);
                    cacheEntry = bay;
                }
                //else
                //{
                //    cacheHit++;
                //}

                return cacheEntry;
            }
        }

        public Bay GetByIdOrDefault(int id)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Bays
                    .AsNoTracking()
                    .SingleOrDefault(b => b.Id == id);
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

                return bay;
            }
        }

        public Bay GetByLoadingUnitLocation(LoadingUnitLocation location)
        {
            lock (this.dataContext)
            {
                this.cache.TryGetValue(GetBayLocationCacheKey(location), out Bay cacheEntry);
                if (cacheEntry is null)
                {
                    var bay = GetByLoadingUnitLocationCompile(this.dataContext, location);

                    if (bay is null)
                    {
                        return null;
                    }
                    this.cache.Set(GetBayLocationCacheKey(location), bay, this.cacheOptions);
                    cacheEntry = bay;
                }
                //else
                //{
                //    cacheHit++;
                //}
                return cacheEntry;
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
                this.cache.TryGetValue(GetBayNumberCacheKey(bayNumber), out Bay cacheEntry);
                if (cacheEntry is null)
                {
                    var bay = GetByNumberCompile(this.dataContext, bayNumber);

                    if (bay is null)
                    {
                        throw new EntityNotFoundException(bayNumber.ToString());
                    }
                    this.cache.Set(GetBayNumberCacheKey(bayNumber), bay, this.cacheOptions);
                    cacheEntry = bay;
                }
                //else
                //{
                //    cacheHit++;
                //}

                return cacheEntry;
            }
        }

        public int GetCarouselBayFindZeroLimit(BayNumber bayNumber) => this.GetByNumber(bayNumber).Carousel.BayFindZeroLimit;

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
                    returnValue = this.GetInverterIndexByNumber(bayNumber);
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
                            returnValue = this.GetShutterInverterIndex(bayNumber);
                            break;

                        case MovementMode.BayChain:
                        case MovementMode.BayChainManual:
                        case MovementMode.BayTest:
                        case MovementMode.BayChainFindZero:
                        case MovementMode.DoubleExtBayTest:
                            returnValue = this.GetInverterIndexByNumber(bayNumber);
                            break;

                        case MovementMode.ExtBayChain:
                        case MovementMode.ExtBayChainManual:
                        case MovementMode.ExtBayTest:
                            returnValue = this.GetInverterIndexByNumber(bayNumber);
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
                    returnValue = this.GetShutterInverterIndex(bayNumber);
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
            lock (this.dataContext)
            {
                return this.GetByNumber(bayNumber).IsExternal;
            }
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
            this.cache.TryGetValue(GetBayPositionLocationCacheKey(location), out BayPosition cacheEntry);
            if (cacheEntry is null)
            {
                var bayPosition = GetPositionByLocationCompile(this.dataContext, location);

                if (bayPosition is null)
                {
                    throw new EntityNotFoundException(location.ToString());
                }
                this.cache.Set(GetBayPositionLocationCacheKey(location), bayPosition, this.cacheOptions);
                cacheEntry = bayPosition;
            }
            //else
            //{
            //    cacheHit++;
            //}

            return cacheEntry;
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
            var shutter = this.GetByNumber(bayNumber).Shutter;
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
                this.RemoveCache(bay.Number);
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

        public void RemoveCache(BayNumber bayNumber)
        {
            this.cache.Remove(GetBayAllCacheKey);
            this.cache.Remove(GetBayCellCacheKey);
            this.cache.Remove(GetBayNumberCacheKey(bayNumber));
            switch (bayNumber)
            {
                case BayNumber.BayOne:
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.CarouselBay1Down));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.CarouselBay1Up));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.ExternalBay1Down));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.ExternalBay1Up));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.InternalBay1Down));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.InternalBay1Up));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.Cell));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.NoLocation));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.LoadUnit));

                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.CarouselBay1Down));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.CarouselBay1Up));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.ExternalBay1Down));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.ExternalBay1Up));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.InternalBay1Down));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.InternalBay1Up));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.Cell));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.NoLocation));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.LoadUnit));

                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.CarouselBay1Down));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.CarouselBay1Up));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.ExternalBay1Down));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.ExternalBay1Up));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.InternalBay1Down));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.InternalBay1Up));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.Cell));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.NoLocation));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.LoadUnit));
                    break;

                case BayNumber.BayTwo:
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.CarouselBay2Down));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.CarouselBay2Up));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.ExternalBay2Down));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.ExternalBay2Up));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.InternalBay2Down));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.InternalBay2Up));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.Cell));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.NoLocation));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.LoadUnit));

                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.CarouselBay2Down));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.CarouselBay2Up));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.ExternalBay2Down));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.ExternalBay2Up));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.InternalBay2Down));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.InternalBay2Up));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.Cell));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.NoLocation));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.LoadUnit));

                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.CarouselBay2Down));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.CarouselBay2Up));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.ExternalBay2Down));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.ExternalBay2Up));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.InternalBay2Down));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.InternalBay2Up));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.Cell));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.NoLocation));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.LoadUnit));
                    break;

                case BayNumber.BayThree:
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.CarouselBay3Down));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.CarouselBay3Up));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.ExternalBay3Down));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.ExternalBay3Up));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.InternalBay3Down));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.InternalBay3Up));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.Cell));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.NoLocation));
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.LoadUnit));

                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.CarouselBay3Down));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.CarouselBay3Up));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.ExternalBay3Down));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.ExternalBay3Up));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.InternalBay3Down));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.InternalBay3Up));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.Cell));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.NoLocation));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.LoadUnit));

                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.CarouselBay3Down));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.CarouselBay3Up));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.ExternalBay3Down));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.ExternalBay3Up));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.InternalBay3Down));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.InternalBay3Up));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.Cell));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.NoLocation));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.LoadUnit));
                    break;

                case BayNumber.ElevatorBay:
                    this.cache.Remove(GetBayPositionCacheKey((int)LoadingUnitLocation.Elevator));
                    this.cache.Remove(GetBayLocationCacheKey(LoadingUnitLocation.Elevator));
                    this.cache.Remove(GetBayPositionLocationCacheKey(LoadingUnitLocation.Elevator));
                    break;
            }
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
                var position = this.GetPositionByLocation(sourceBay);
                this.SetLoadingUnit(position.Id, null);
                this.RemovePositionCache(position.Location);
            }

            lock (this.dataContext)
            {
                this.dataContext.LoadingUnits.Remove(lu);
                this.dataContext.SaveChanges();

                this.NotifyRemoveLoadUnit(loadingUnitId, sourceBay);
            }
        }

        public void RemovePositionCache(LoadingUnitLocation location)
        {
            this.cache.Remove(GetBayAllCacheKey);
            this.cache.Remove(GetBayCellCacheKey);
            this.cache.Remove(GetBayPositionCacheKey((int)location));
            this.cache.Remove(GetBayLocationCacheKey(location));
            this.cache.Remove(GetBayPositionLocationCacheKey(location));
            switch (location)
            {
                case LoadingUnitLocation.CarouselBay1Down:
                case LoadingUnitLocation.CarouselBay1Up:
                case LoadingUnitLocation.ExternalBay1Down:
                case LoadingUnitLocation.ExternalBay1Up:
                case LoadingUnitLocation.InternalBay1Down:
                case LoadingUnitLocation.InternalBay1Up:
                    this.cache.Remove(GetBayNumberCacheKey(BayNumber.BayOne));
                    break;

                case LoadingUnitLocation.CarouselBay2Down:
                case LoadingUnitLocation.CarouselBay2Up:
                case LoadingUnitLocation.ExternalBay2Down:
                case LoadingUnitLocation.ExternalBay2Up:
                case LoadingUnitLocation.InternalBay2Down:
                case LoadingUnitLocation.InternalBay2Up:
                    this.cache.Remove(GetBayNumberCacheKey(BayNumber.BayTwo));
                    break;

                case LoadingUnitLocation.CarouselBay3Down:
                case LoadingUnitLocation.CarouselBay3Up:
                case LoadingUnitLocation.ExternalBay3Down:
                case LoadingUnitLocation.ExternalBay3Up:
                case LoadingUnitLocation.InternalBay3Down:
                case LoadingUnitLocation.InternalBay3Up:
                    this.cache.Remove(GetBayNumberCacheKey(BayNumber.BayThree));
                    break;

                case LoadingUnitLocation.LoadUnit:
                case LoadingUnitLocation.Cell:
                case LoadingUnitLocation.NoLocation:
                    this.cache.Remove(GetBayNumberCacheKey(BayNumber.BayOne));
                    this.cache.Remove(GetBayNumberCacheKey(BayNumber.BayTwo));
                    this.cache.Remove(GetBayNumberCacheKey(BayNumber.BayThree));
                    break;

                case LoadingUnitLocation.Elevator:
                    this.cache.Remove(GetBayNumberCacheKey(BayNumber.ElevatorBay));
                    break;
            }
        }

        public void ResetMachine()
        {
            lock (this.dataContext)
            {
                foreach (var bayPosition in this.dataContext.BayPositions.Include(i => i.LoadingUnit)
)
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
                        this.RemovePositionCache(bayPosition.Location);
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

        public void SetAllOperationsBay(bool pick, bool put, bool view, bool inventory, bool barcodeAutomaticPut, int bayid, bool showBarcodeImage, bool checkListContinueInOtherMachine)
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

                this.dataContext.SaveChanges();
                this.RemoveCache(bay.Number);
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
                this.RemoveCache(bay.Number);

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
                this.RemoveCache(bay.Number);

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
                this.RemoveCache(bayNumber);
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
            this.RemovePositionCache(position.Location);
        }

        public void SetProfileConstBay(BayNumber bayNumber, double k0, double k1)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays.SingleOrDefault(b => b.Number == bayNumber);
                bay.ProfileConst0 = k0;
                bay.ProfileConst1 = k1;

                this.dataContext.SaveChanges();
                this.RemoveCache(bay.Number);
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
                    this.RemoveCache(bay.Number);
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
                this.RemoveCache(bay.Number);
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
                    this.RemoveCache(bay.Number);
                }
            }
        }

        public void UpdateExtraRace(BayNumber bayNumber, double extraRace)
        {
            lock (this.dataContext)
            {
                var bay = this.GetByNumber(bayNumber);
                if (bay.External != null)
                {
                    bay.External.ExtraRace = extraRace;
                    this.dataContext.AddOrUpdate(bay.External, f => f.Id);
                    this.dataContext.SaveChanges();
                    this.RemoveCache(bay.Number);
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
                this.RemoveCache(bay.Number);
            }
        }

        public void UpdateLastIdealPosition(double position, BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                    .Include(b => b.Carousel)
                    .Include(b => b.External)
                    .SingleOrDefault(b => b.Number == bayNumber);
                if (bay is null)
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }

                if (bay.Carousel != null)
                {
                    // Handle the carousel
                    bay.Carousel.LastIdealPosition = position;
                    this.dataContext.SaveChanges();
                    this.RemoveCache(bay.Number);
                }

                if (bay.External != null)
                {
                    // Handle the external bay
                    bay.External.LastIdealPosition = position;
                    this.dataContext.SaveChanges();
                    this.RemoveCache(bay.Number);
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

                this.RemoveCache(bay.Number);
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
                    this.RemoveCache(bay.Number);
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
                    this.RemoveCache(bay.Number);
                }
            }
        }

        internal static string GetBayLocationCacheKey(LoadingUnitLocation location) => $"BayLocationKey{location}";

        internal static string GetBayNumberCacheKey(BayNumber bayNumber) => $"BayNumberKey{bayNumber}";

        internal static string GetBayPositionCacheKey(int bayPosition) => $"BayPositionKey{bayPosition}";

        internal static string GetBayPositionLocationCacheKey(LoadingUnitLocation location) => $"BayPositionLocationKey{location}";

        internal static string GetInverterIndexCacheKey(InverterIndex inverterIndex) => $"{nameof(GetByInverterIndex)}{inverterIndex}";

        private InverterIndex GetInverterIndexByNumber(BayNumber bayNumber)
        {
            return this.GetByNumber(bayNumber).Inverter.Index;
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
                this.RemoveCache(bay.Number);
            }
        }

        #endregion
    }
}
