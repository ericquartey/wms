using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class ElevatorDataProvider : IElevatorDataProvider
    {
        #region Fields

        internal const string ElevatorCurrentBayPositionCacheKey = "ElevatorCurrentBayPositionCacheKey";

        internal const string ElevatorCurrentCellCacheKey = "ElevatorCurrentCellCacheKey";

        /// <summary>
        /// The position tolerance, in millimeters, used to validate the logical position of the elevator when located opposite a bay or a cell.
        /// </summary>
        private const double VerticalPositionValidationTolerance = 7.5;

        private static readonly Func<DataLayerContext, BayPosition> GetCurrentBayPositionCompile =
                EF.CompileQuery((DataLayerContext context) =>
                context.Elevators
                    .AsNoTracking()
                    .Include(p => p.BayPosition).ThenInclude(p => p.LoadingUnit)
                    .Include(p => p.BayPosition).ThenInclude(p => p.Bay)
                    .Select(e => e.BayPosition)
                    .Single());

        private static readonly Func<DataLayerContext, Cell> GetCurrentCellCompile =
                EF.CompileQuery((DataLayerContext context) =>
                context.Elevators
                    .AsNoTracking()
                    .Include(p => p.Cell).ThenInclude(p => p.Panel)
                    .Include(p => p.Cell).ThenInclude(p => p.LoadingUnit)
                   .Select(e => e.Cell)
                   .Single());

        private static readonly Func<DataLayerContext, Elevator> GetLoadingUnitOnBoardCompile =
                EF.CompileQuery((DataLayerContext context) =>
                context.Elevators
                    .AsNoTracking()
                    .Include(e => e.LoadingUnit)
                    .ThenInclude(l => l.Cell)
                    .ThenInclude(c => c.Panel)
                    .Single());

        private static readonly Func<DataLayerContext, Elevator> GetStructuralPropertiesCompile =
                EF.CompileQuery((DataLayerContext context) =>
                context.Elevators
                    .AsNoTracking()
                    .Include(e => e.StructuralProperties)
                    .Single());

        private readonly IMemoryCache cache;

        private readonly MemoryCacheEntryOptions cacheOptions;

        private readonly DataLayerContext dataContext;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger<DataLayerContext> logger;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public ElevatorDataProvider(
            DataLayerContext dataContext,
            IMemoryCache memoryCache,
            IConfiguration configuration,
            IEventAggregator eventAggregator,
            IMachineVolatileDataProvider machineVolatileDataProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ILogger<DataLayerContext> logger)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.machineVolatileDataProvider = machineVolatileDataProvider ?? throw new ArgumentNullException(nameof(machineVolatileDataProvider));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            this.cacheOptions = configuration.GetMemoryCacheOptions();
        }

        #endregion

        #region Properties

        public double HorizontalPosition
        {
            get => this.machineVolatileDataProvider.ElevatorHorizontalPosition;
            set
            {
                if (this.machineVolatileDataProvider.ElevatorHorizontalPosition != value)
                {
                    this.machineVolatileDataProvider.ElevatorHorizontalPosition = value;

                    this.NotifyElevatorPositionChanged(useCachedValue: true);
                }
            }
        }

        public double VerticalPosition
        {
            get => this.machineVolatileDataProvider.ElevatorVerticalPosition;
            set
            {
                if (this.machineVolatileDataProvider.ElevatorVerticalPosition != value)
                {
                    this.machineVolatileDataProvider.ElevatorVerticalPosition = value;

                    this.cache.TryGetValue(ElevatorCurrentCellCacheKey, out Cell currentCell);
                    this.cache.TryGetValue(ElevatorCurrentBayPositionCacheKey, out BayPosition currentBayPosition);

                    if (currentCell != null && this.IsVerticalPositionWithinTolerance(currentCell.Position))
                    {
                        this.NotifyElevatorPositionChanged(useCachedValue: true);
                    }
                    else if (currentBayPosition != null && this.IsVerticalPositionWithinTolerance(currentBayPosition.Height))
                    {
                        this.NotifyElevatorPositionChanged(useCachedValue: true);
                    }
                    else
                    {
                        this.NotifyElevatorPositionChanged();
                    }
                }
            }
        }

        #endregion

        #region Methods

        public void CompleteProcedure()
        {
            lock (this.dataContext)
            {
                var procedureParameters = this.setupProceduresDataProvider.GetVerticalOffsetCalibration();
                this.setupProceduresDataProvider.MarkAsCompleted(procedureParameters);
            }
        }

        public ElevatorAxisManualParameters GetAssistedMovementsAxis(Orientation orientation) => this.GetAxis(orientation).AssistedMovements;

        public ElevatorAxis GetAxis(Orientation orientation)
        {
            lock (this.dataContext)
            {
                var cacheKey = GetAxisCacheKey(orientation);
                if (!this.cache.TryGetValue(cacheKey, out ElevatorAxis cacheEntry))
                {
                    cacheEntry = this.dataContext.ElevatorAxes.AsNoTracking()
                        .Include(a => a.Profiles)
                        .ThenInclude(p => p.Steps)
                        .Include(a => a.FullLoadMovement)
                        .Include(a => a.EmptyLoadMovement)
                        .Include(a => a.ManualMovements)
                        .Include(a => a.AssistedMovements)
                        .Include(a => a.WeightMeasurement)
                        .SingleOrDefault(a => a.Orientation == orientation);

                    if (cacheEntry is null)
                    {
                        throw new EntityNotFoundException(orientation.ToString());
                    }

                    this.cache.Set(cacheKey, cacheEntry, this.cacheOptions);
                }

                return cacheEntry.Clone();
            }
        }

        public BayPosition GetCachedCurrentBayPosition()
        {
            this.cache.TryGetValue(ElevatorCurrentBayPositionCacheKey, out BayPosition cacheEntry);
            return cacheEntry;
        }

        public Cell GetCachedCurrentCell()
        {
            this.cache.TryGetValue(ElevatorCurrentCellCacheKey, out Cell cacheEntry);
            return cacheEntry;
        }

        public IDbContextTransaction GetContextTransaction()
        {
            return this.dataContext.Database.BeginTransaction();
        }

        public BayPosition GetCurrentBayPosition()
        {
            lock (this.dataContext)
            {
                try
                {
                    var currentBayPosition = GetCurrentBayPositionCompile(this.dataContext);
                    return currentBayPosition;
                }
                catch (EntityNotFoundException)
                {
                    return null;
                }
            }
        }

        public Cell GetCurrentCell()
        {
            lock (this.dataContext)
            {
                try
                {
                    var currentCell = GetCurrentCellCompile(this.dataContext);

                    return currentCell;
                }
                catch (EntityNotFoundException)
                {
                    return null;
                }
            }
        }

        public int GetCyclesFromCalibrationHorizontal()
        {
            lock (this.dataContext)
            {
                var axis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == Orientation.Horizontal);
                if (axis is null)
                {
                    throw new EntityNotFoundException(Orientation.Horizontal.ToString());
                }
                var cycles = this.dataContext.LastOrNull(this.dataContext.MachineStatistics, o => o.Id)?.Entity?.TotalHorizontalAxisCycles ?? 0;
                return Math.Abs(cycles - axis.LastCalibrationCycles);
            }
        }

        public int GetCyclesFromCalibrationVertical()
        {
            lock (this.dataContext)
            {
                var axis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == Orientation.Vertical);
                if (axis is null)
                {
                    throw new EntityNotFoundException(Orientation.Vertical.ToString());
                }
                var cycles = this.dataContext.LastOrNull(this.dataContext.MachineStatistics, o => o.Id)?.Entity?.TotalVerticalAxisCycles ?? 0;
                return Math.Abs(cycles - axis.LastCalibrationCycles);
            }
        }

        public IEnumerable<ElevatorAxis> GetElevatorAxes()
        {
            lock (this.dataContext)
            {
                var cacheKey = GetAxesCacheKey();
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

        public LoadingUnit GetLoadingUnitOnBoard()
        {
            lock (this.dataContext)
            {
                var elevator = GetLoadingUnitOnBoardCompile(this.dataContext);

                return elevator.LoadingUnit;
            }
        }

        public ElevatorAxisManualParameters GetManualMovementsAxis(Orientation orientation) => this.GetAxis(orientation).ManualMovements;

        public ElevatorStructuralProperties GetStructuralProperties()
        {
            lock (this.dataContext)
            {
                var elevator = GetStructuralPropertiesCompile(this.dataContext);

                return elevator.StructuralProperties;
            }
        }

        public double GetWeight()
        {
            lock (this.dataContext)
            {
                var elevator = this.dataContext.Elevators
                    .Include(e => e.StructuralProperties)
                    .Single();

                return elevator.StructuralProperties.ElevatorWeight;
            }
        }

        public bool IsVerticalPositionWithinTolerance(double position)
        {
            return
                this.machineVolatileDataProvider.ElevatorVerticalPosition - VerticalPositionValidationTolerance < position
                &&
                this.machineVolatileDataProvider.ElevatorVerticalPosition + VerticalPositionValidationTolerance > position;
        }

        public void ResetMachine()
        {
            lock (this.dataContext)
            {
                var elevator = this.dataContext.Elevators.Single();

                var lu = this.dataContext.LoadingUnits.SingleOrDefault(l => l.Id == elevator.LoadingUnitId);
                if (lu != null)
                {
                    lu.Status = DataModels.Enumerations.LoadingUnitStatus.Undefined;
                    this.dataContext.LoadingUnits.Update(lu);
                }

                elevator.LoadingUnit = null;
                elevator.LoadingUnitId = null;

                this.dataContext.SaveChanges();

                this.cache.Remove(ElevatorCurrentBayPositionCacheKey);
                this.cache.Remove(ElevatorCurrentCellCacheKey);
                this.cache.Remove(GetAxisCacheKey(Orientation.Horizontal));
                this.cache.Remove(GetAxisCacheKey(Orientation.Vertical));
                this.cache.Remove(GetAxesCacheKey());
            }
        }

        public MovementParameters ScaleMovementsByWeight(Orientation orientation, bool isLoadingUnitOnBoard)
        {
            var axis = this.GetAxis(orientation);

            var loadingUnit = this.GetLoadingUnitOnBoard();

            return axis.ScaleMovementsByWeight(loadingUnit, isLoadingUnitOnBoard);
        }

        public void SetCurrentBayPosition(int? bayPositionId)
        {
            lock (this.dataContext)
            {
                var currentBayPosition = this.GetCachedCurrentBayPosition();

                var elevator = this.dataContext.Elevators
                    .Include(e => e.BayPosition)
                    .Include(e => e.Cell)
                    .ThenInclude(c => c.Panel)
                    .Single();

                if (currentBayPosition is null || currentBayPosition.Id != bayPositionId)
                {
                    if (bayPositionId.HasValue)
                    {
                        var newBayPosition = this.dataContext.BayPositions.SingleOrDefault(b => b.Id == bayPositionId);
                        if (newBayPosition is null)
                        {
                            throw new EntityNotFoundException(bayPositionId.Value);
                        }

                        elevator.BayPosition = newBayPosition;
                        this.cache.Set(ElevatorCurrentBayPositionCacheKey, newBayPosition, this.cacheOptions);
                        elevator.Cell = null;
                        this.cache.Remove(ElevatorCurrentCellCacheKey);
                    }
                    else
                    {
                        elevator.BayPosition = null;
                        this.cache.Remove(ElevatorCurrentBayPositionCacheKey);
                    }

                    this.dataContext.SaveChanges();
                    this.NotifyElevatorPositionChanged(useCachedValue: true);
                }
            }
        }

        public void SetCurrentCell(int? cellId)
        {
            lock (this.dataContext)
            {
                var currentCell = this.GetCachedCurrentCell();

                var elevator = this.dataContext.Elevators
                    .Include(e => e.BayPosition)
                    .Include(e => e.Cell)
                    .ThenInclude(c => c.Panel)
                    .Single();

                if (currentCell is null || currentCell.Id != cellId)
                {
                    if (cellId.HasValue)
                    {
                        var newCell = this.dataContext.Cells.Include(c => c.Panel).SingleOrDefault(c => c.Id == cellId);
                        if (newCell is null)
                        {
                            throw new EntityNotFoundException(cellId.Value);
                        }

                        elevator.Cell = newCell;
                        this.cache.Set(ElevatorCurrentCellCacheKey, newCell, this.cacheOptions);
                        elevator.BayPosition = null;
                        this.cache.Remove(ElevatorCurrentBayPositionCacheKey);
                    }
                    else
                    {
                        elevator.Cell = null;
                        this.cache.Remove(ElevatorCurrentCellCacheKey);
                    }

                    this.dataContext.SaveChanges();
                    this.NotifyElevatorPositionChanged(useCachedValue: true);
                }
            }
        }

        public void SetLoadingUnit(int? loadingUnitId)
        {
            lock (this.dataContext)
            {
                var elevator = this.dataContext.Elevators.Single();

                elevator.LoadingUnitId = loadingUnitId;

                if (loadingUnitId.HasValue)
                {
                    var loadingUnit = this.dataContext.LoadingUnits.SingleOrDefault(e => e.Id == loadingUnitId);
                    if (loadingUnit != null)
                    {
                        loadingUnit.Status = DataModels.Enumerations.LoadingUnitStatus.InElevator;
                    }
                }

                this.dataContext.SaveChanges();
            }
        }

        public void UpdateHorizontalDistance(double newDistance)
        {
            lock (this.dataContext)
            {
                var axis = this.dataContext.ElevatorAxes.SingleOrDefault(e => e.Orientation == Orientation.Horizontal);

                axis.ChainOffset = Math.Round(newDistance, 2);

                //foreach (var profile in axis.Profiles)
                //{
                //    profile.TotalDistance = newDistance;
                //    var lastStep = profile.Steps.OrderBy(x => x.Number).Last();
                //    lastStep.Position = newDistance;
                //}
                this.dataContext.ElevatorAxes.Update(axis);
                this.dataContext.SaveChanges();

                var cacheKey = GetAxisCacheKey(Orientation.Horizontal);
                this.cache.Remove(cacheKey);
            }
        }

        public void UpdateHorizontalResolution(double newResolution)
        {
            lock (this.dataContext)
            {
                var axis = this.dataContext.ElevatorAxes.SingleOrDefault(e => e.Orientation == Orientation.Horizontal);

                axis.Resolution = newResolution;
                this.dataContext.ElevatorAxes.Update(axis);
                this.dataContext.SaveChanges();

                var cacheKey = GetAxisCacheKey(Orientation.Horizontal);
                this.cache.Remove(cacheKey);

                this.logger.LogDebug($"Elevator axis Horizontal resolution save {newResolution}");
            }
        }

        public void UpdateLastCalibrationCycles(Orientation orientation)
        {
            lock (this.dataContext)
            {
                var axis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == orientation);
                if (axis is null)
                {
                    throw new EntityNotFoundException(orientation.ToString());
                }

                this.cache.Remove(GetAxisCacheKey(orientation));

                if (orientation == Orientation.Vertical)
                {
                    var cycles = this.dataContext.LastOrNull(this.dataContext.MachineStatistics, o => o.Id)?.Entity?.TotalVerticalAxisCycles ?? 0;
                    axis.LastCalibrationCycles = cycles;
                }
                else if (orientation == Orientation.Horizontal)
                {
                    var cycles = this.dataContext.LastOrNull(this.dataContext.MachineStatistics, o => o.Id)?.Entity?.TotalHorizontalAxisCycles ?? 0;
                    axis.LastCalibrationCycles = cycles;
                }

                this.dataContext.SaveChanges();
                this.NotifyElevatorPositionChanged(useCachedValue: true);
            }
        }

        public void UpdateLastIdealPosition(double position, Orientation orientation = Orientation.Horizontal, int tolerance = 1)
        {
            lock (this.dataContext)
            {
                if (Math.Abs(position - this.GetAxis(orientation).LastIdealPosition) > tolerance)
                {
                    var posAxis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == orientation);

                    posAxis.LastIdealPosition = position;

                    this.dataContext.ElevatorAxes.Update(posAxis);
                    this.dataContext.SaveChanges();
                    this.logger.LogDebug($"Elevator axis {orientation} last position save {position:0.00}");

                    // reload cache
                    this.cache.Remove(GetAxisCacheKey(orientation));
                    _ = this.GetAxis(orientation);

                    if (orientation == Orientation.Horizontal)
                    {
                        this.NotifyElevatorPositionChanged(useCachedValue: true);
                    }
                }
            }
        }

        public void UpdateMeasureConst(double measureConst0, double measureConst1, double measureConst2)
        {
            lock (this.dataContext)
            {
                var axis = this.dataContext.ElevatorAxes.Include(a => a.WeightMeasurement).SingleOrDefault(e => e.Orientation == Orientation.Vertical);

                axis.WeightMeasurement.MeasureConst0 = measureConst0;

                axis.WeightMeasurement.MeasureConst1 = measureConst1;

                axis.WeightMeasurement.MeasureConst2 = measureConst2;

                this.dataContext.ElevatorAxes.Update(axis);
                this.dataContext.SaveChanges();

                var cacheKey = GetAxisCacheKey(Orientation.Vertical);
                this.cache.Remove(cacheKey);
            }
        }

        public void UpdateVerticalLowerBound(double newLowerBound)
        {
            lock (this.dataContext)
            {
                var axis = this.dataContext.ElevatorAxes.SingleOrDefault(e => e.Orientation == Orientation.Vertical);

                axis.LowerBound = newLowerBound;
                this.dataContext.ElevatorAxes.Update(axis);
                this.dataContext.SaveChanges();

                var cacheKey = GetAxisCacheKey(Orientation.Vertical);
                this.cache.Remove(cacheKey);
            }
        }

        public void UpdateVerticalOffset(double newOffset)
        {
            lock (this.dataContext)
            {
                var verticalAxis = this.dataContext.ElevatorAxes.SingleOrDefault(e => e.Orientation == Orientation.Vertical);

                verticalAxis.Offset = newOffset;
                this.dataContext.ElevatorAxes.Update(verticalAxis);
                this.dataContext.SaveChanges();
                this.logger.LogDebug($"Elevator axis vertical offset save {newOffset}");

                var cacheKey = GetAxisCacheKey(Orientation.Vertical);
                this.cache.Remove(cacheKey);
            }
        }

        public void UpdateVerticalOffsetAndComplete(double newOffset)
        {
            lock (this.dataContext)
            {
                var verticalAxis = this.dataContext.ElevatorAxes.SingleOrDefault(e => e.Orientation == Orientation.Vertical);

                verticalAxis.Offset = newOffset;
                this.dataContext.ElevatorAxes.Update(verticalAxis);
                this.dataContext.SaveChanges();
                this.logger.LogDebug($"Elevator axis vertical offset save {newOffset}");

                var cacheKey = GetAxisCacheKey(Orientation.Vertical);
                this.cache.Remove(cacheKey);

                var procedureParameters = this.setupProceduresDataProvider.GetVerticalOffsetCalibration();
                this.setupProceduresDataProvider.MarkAsCompleted(procedureParameters);
            }
        }

        public void UpdateVerticalResolution(double newResolution)
        {
            lock (this.dataContext)
            {
                var verticalAxis = this.dataContext.ElevatorAxes.SingleOrDefault(e => e.Orientation == Orientation.Vertical);

                verticalAxis.Resolution = newResolution;
                this.dataContext.ElevatorAxes.Update(verticalAxis);
                this.dataContext.SaveChanges();
                this.logger.LogDebug($"Elevator axis vertical resolution save {newResolution}");

                var cacheKey = GetAxisCacheKey(Orientation.Vertical);
                this.cache.Remove(cacheKey);

                var procedureParameters = this.setupProceduresDataProvider.GetVerticalResolutionCalibration();
                this.setupProceduresDataProvider.MarkAsCompleted(procedureParameters);
            }
        }

        internal static string GetAxesCacheKey() => $"{nameof(GetElevatorAxes)}";

        internal static string GetAxisCacheKey(Orientation orientation) => $"{nameof(GetAxis)}{orientation}";

        private void NotifyElevatorPositionChanged(bool useCachedValue = false)
        {
            BayPosition bayPosition = null;
            Cell cellPosition = null;

            if (useCachedValue)
            {
                this.cache.TryGetValue(ElevatorCurrentBayPositionCacheKey, out bayPosition);
                this.cache.TryGetValue(ElevatorCurrentCellCacheKey, out cellPosition);
            }

            this.eventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(
                    new NotificationMessage
                    {
                        Data = new ElevatorPositionMessageData(
                            this.machineVolatileDataProvider.ElevatorVerticalPosition,
                            this.machineVolatileDataProvider.ElevatorHorizontalPosition,
                            cellPosition?.Id,
                            bayPosition?.Id,
                            bayPosition?.IsUpper),
                        Destination = MessageActor.Any,
                        Source = MessageActor.DataLayer,
                        Type = MessageType.ElevatorPosition,
                    });
        }

        #endregion
    }
}
