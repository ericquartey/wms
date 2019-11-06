using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class BaysProvider : BaseProvider, IBaysProvider
    {
        #region Fields

        private static readonly MemoryCacheEntryOptions CacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(1));

        private readonly IMemoryCache cache;

        private readonly DataLayerContext dataContext;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly double kMul = 0.090625;

        private readonly double kSum = -181.25;

        private readonly IMachineProvider machineProvider;

        private readonly NotificationEvent notificationEvent;

        #endregion

        #region Constructors

        public BaysProvider(
            DataLayerContext dataContext,
            IEventAggregator eventAggregator,
            IMachineProvider machineProvider,
            IElevatorDataProvider elevatorDataProvider,
            IMemoryCache memoryCache)
            : base(eventAggregator)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
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

                bay.IsActive = true;

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

        public Bay AssignMissionOperation(BayNumber bayNumber, int? missionId, int? missionOperationId)
        {
            lock (this.dataContext)
            {
                var bay = this.GetByNumber(bayNumber);
                if (bay is null)
                {
                    throw new EntityNotFoundException(bayNumber.ToString());
                }

                bay.CurrentMissionId = missionId;
                bay.CurrentMissionOperationId = missionOperationId;

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
        public double ConvertProfileToHeight(ushort profile)
        {
            return (profile * this.kMul) + this.kSum;
        }

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

                bay.IsActive = false;

                this.Update(bay);

                return bay;
            }
        }

        public IEnumerable<Bay> GetAll()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Bays
                    .Include(b => b.Inverter)
                    .Include(b => b.Positions)
                    .ThenInclude(s => s.LoadingUnit)
                    .Include(b => b.Shutter)
                    .ThenInclude(s => s.Inverter)
                    .Include(b => b.Carousel)
                    .ToArray();
            }
        }

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

        public BayNumber GetByInverterIndex(InverterIndex inverterIndex)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays.SingleOrDefault(b =>
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
                        throw new ArgumentOutOfRangeException($"No inverter with index {inverterIndex} is configured.");
                    }
                }

                return bay.Number;
            }
        }

        public BayNumber GetByIoIndex(IoIndex ioIndex, FieldMessageType messageType)
        {
            // Hack required to handle exceptions (like axis switch on 800Kg machine) in order to fix device/bay association
            if (messageType == FieldMessageType.SwitchAxis)
            {
                return BayNumber.ElevatorBay;
            }

            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
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
                    .Include(b => b.Inverter)
                    .Include(b => b.Positions)
                    .ThenInclude(s => s.LoadingUnit)
                    .Include(b => b.Shutter)
                    .ThenInclude(s => s.Inverter)
                    .Include(b => b.Carousel)
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
            return this.dataContext.Bays.FirstOrDefault(b => b.Positions.Any(p => p.Location == location));
        }

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
                case MovementMode.PositionAndMeasure:
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
                    .Include(b => b.Carousel)
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
                var bay = this.dataContext.Bays
                    .SingleOrDefault(b => b.Inverter.Index == inverterIndex);

                if (bay is null)
                {
                    throw new EntityNotFoundException(inverterIndex.ToString());
                }

                return bay.ChainOffset;
            }
        }

        public IEnumerable<ElevatorAxis> GetElevatorAxes()
        {
            lock (this.dataContext)
            {
                var cacheKey = GetElevatorAxesCacheKey();
                if (!this.cache.TryGetValue(cacheKey, out IEnumerable<ElevatorAxis> cacheEntry))
                {
                    cacheEntry = this.dataContext.ElevatorAxes
                        .Include(i => i.Inverter)
                        .ToList();
                    if (cacheEntry is null)
                    {
                        throw new EntityNotFoundException(string.Empty);
                    }

                    this.cache.Set(cacheKey, cacheEntry, CacheOptions);
                }

                return cacheEntry;
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
                    returnValue = this.GetByNumber(bayNumber).Inverter.Index;
                    break;

                default:
                    break;
            }

            return returnValue;
        }

        public InverterIndex GetInverterIndexByMovementType(IPositioningMessageData data, BayNumber bayNumber)
        {
            var returnValue = InverterIndex.None;

            switch (bayNumber)
            {
                case BayNumber.ElevatorBay:
                    switch (data.MovementMode)
                    {
                        case MovementMode.Position:
                        case MovementMode.PositionAndMeasure:
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
            return this.dataContext.BayPositions
                       .Where(p => p.Location == location)
                       .Select(p => p.LoadingUnit).SingleOrDefault();
        }

        public double? GetLoadingUnitDestinationHeight(LoadingUnitLocation location)
        {
            return this.dataContext.BayPositions.SingleOrDefault(p => p.Location == location)?.Height;
        }

        public LoadingUnitLocation GetLoadingUnitLocationByLoadingUnit(int loadingUnitId)
        {
            return this.dataContext.BayPositions.SingleOrDefault(p => p.LoadingUnit.Id == loadingUnitId)?.Location ?? LoadingUnitLocation.NoLocation;
        }

        public LoadingUnitLocation GetPositionByHeight(double position, double tolerance, BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                return this.dataContext.Bays
                           .Where(b => b.Number == bayNumber)
                           .SelectMany(b => b.Positions)
                           .SingleOrDefault(p => p.Height > position - tolerance && p.Height < position + tolerance)?.Location ?? LoadingUnitLocation.NoLocation;
            }
        }

        public double GetResolution(InverterIndex inverterIndex)
        {
            lock (this.dataContext)
            {
                var bay = this.dataContext.Bays
                              .SingleOrDefault(b => b.Inverter.Index == inverterIndex);

                if (bay is null)
                {
                    throw new EntityNotFoundException(inverterIndex.ToString());
                }

                return bay.Resolution;
            }
        }

        public void LoadLoadingUnit(int loadingUnitId, LoadingUnitLocation destination)
        {
            var position = this.dataContext.BayPositions.Single(p => p.Location == destination);
            var loadingUnit = this.dataContext.LoadingUnits.Single(l => l.Id == loadingUnitId);

            position.LoadingUnit = loadingUnit;

            this.dataContext.BayPositions.Update(position);
            this.dataContext.SaveChanges();
        }

        public void ResetMachine()
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
                bay.CurrentMissionId = null;
                bay.CurrentMissionOperationId = null;
                this.Update(bay);
            }
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

        public void UnloadLoadingUnit(LoadingUnitLocation destination)
        {
            var position = this.dataContext.BayPositions.Single(p => p.Location == destination);
            position.LoadingUnit = null;

            this.dataContext.BayPositions.Update(position);
            this.dataContext.SaveChanges();
        }

        public Bay UpdatePosition(BayNumber bayNumber, int positionIndex, double height)
        {
            lock (this.dataContext)
            {
                var bay = this.GetByNumber(bayNumber);
                if (positionIndex < 0 || positionIndex > bay.Positions.Count())
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
        }

        internal static string GetElevatorAxesCacheKey() => $"{nameof(GetElevatorAxes)}";

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
                    MessageActor.MachineManager,
                    MessageActor.WebApi,
                    MessageType.BayOperationalStatusChanged,
                    bay.Number));
        }

        #endregion
    }
}
