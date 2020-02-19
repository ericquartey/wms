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
        /// The average vertical spacing between two cells, in millimeters.
        /// </summary>
        private const double CellHeight = 25.0;

        /// <summary>
        /// The position tolerance, in millimeters, used to validate the logical position of the elevator when located opposite a bay or a cell.
        /// </summary>
        private const double VerticalPositionValidationTolerance = 7.5;

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

                return cacheEntry;
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
                var currentBayPosition = this.dataContext.Elevators
                    .Select(e => e.BayPosition)
                        .Include(p => p.LoadingUnit)
                        .Include(p => p.Bay)
                    .SingleOrDefault();

                return currentBayPosition;
            }
        }

        public Cell GetCurrentCell()
        {
            lock (this.dataContext)
            {
                var currentCell = this.dataContext.Elevators
                   .Select(e => e.Cell)
                   .Include(c => c.Panel)
                   .Include(c => c.LoadingUnit)
                   .SingleOrDefault();

                return currentCell;
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
                var elevator = this.dataContext.Elevators.AsNoTracking()
                    .Include(e => e.LoadingUnit)
                    .ThenInclude(l => l.Cell)
                    .ThenInclude(c => c.Panel)
                    .Single();

                return elevator.LoadingUnit;
            }
        }

        public ElevatorAxisManualParameters GetManualMovementsAxis(Orientation orientation) => this.GetAxis(orientation).ManualMovements;

        public ElevatorStructuralProperties GetStructuralProperties()
        {
            lock (this.dataContext)
            {
                var elevator = this.dataContext.Elevators
                    .Include(e => e.StructuralProperties)
                    .Single();

                return elevator.StructuralProperties;
            }
        }

        public bool IsVerticalPositionWithinTolerance(double position)
        {
            return
                this.machineVolatileDataProvider.ElevatorVerticalPosition - VerticalPositionValidationTolerance < position
                &&
                this.machineVolatileDataProvider.ElevatorVerticalPosition + VerticalPositionValidationTolerance > position;
        }

        public void LoadLoadingUnit(int id)
        {
            lock (this.dataContext)
            {
                var elevator = this.dataContext
                    .Elevators
                    .Include(e => e.LoadingUnit)
                    .Single();

                elevator.LoadingUnitId = id;

                this.dataContext.SaveChanges();
            }
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
                var currentBayPosition = this.GetCurrentBayPosition();

                var elevator = this.dataContext.Elevators
                    .Include(e => e.BayPosition)
                    .Single();

                if (currentBayPosition?.Id != bayPositionId)
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
                var currentCell = this.GetCurrentCell();

                var elevator = this.dataContext.Elevators
                    .Include(e => e.Cell)
                    .ThenInclude(c => c.Panel)
                    .Single();

                if (currentCell?.Id != cellId)
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

                this.dataContext.SaveChanges();
            }
        }

        public void UpdateLastIdealPosition(double position, Orientation orientation = Orientation.Horizontal)
        {
            lock (this.dataContext)
            {
                var axis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == orientation);
                if (axis is null)
                {
                    throw new EntityNotFoundException(orientation.ToString());
                }

                axis.LastIdealPosition = position;

                this.dataContext.SaveChanges();

                this.cache.Remove(GetAxisCacheKey(orientation));
                if (orientation == Orientation.Horizontal)
                {
                    this.NotifyElevatorPositionChanged(useCachedValue: true);
                }
            }
        }

        public void UpdateVerticalOffset(double newOffset)
        {
            lock (this.dataContext)
            {
                var cacheKey = GetAxisCacheKey(Orientation.Vertical);
                this.cache.Remove(cacheKey);

                var verticalAxis = this.GetAxis(Orientation.Vertical);

                verticalAxis.Offset = newOffset;
                this.dataContext.ElevatorAxes.Update(verticalAxis);
                this.dataContext.SaveChanges();

                var procedureParameters = this.setupProceduresDataProvider.GetVerticalOffsetCalibration();
                this.setupProceduresDataProvider.MarkAsCompleted(procedureParameters);
            }
        }

        public void UpdateVerticalResolution(double newResolution)
        {
            lock (this.dataContext)
            {
                var cacheKey = GetAxisCacheKey(Orientation.Vertical);
                this.cache.Remove(cacheKey);

                var verticalAxis = this.GetAxis(Orientation.Vertical);

                verticalAxis.Resolution = newResolution;
                this.dataContext.ElevatorAxes.Update(verticalAxis);
                this.dataContext.SaveChanges();

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
