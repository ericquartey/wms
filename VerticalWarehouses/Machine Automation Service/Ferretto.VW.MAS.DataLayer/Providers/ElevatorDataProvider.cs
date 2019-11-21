using System;
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
        /// The position tolerance, in millimeters, used to validate the logical positionin of the elevator when located opposite a bay or a cell.
        /// </summary>
        private const double VerticalPositionValidationTolerance = 5;

        private readonly IMemoryCache cache;

        private readonly MemoryCacheEntryOptions cacheOptions;

        private readonly DataLayerContext dataContext;

        private readonly IElevatorVolatileDataProvider elevatorVolatileDataProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger<ElevatorDataProvider> logger;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public ElevatorDataProvider(
            DataLayerContext dataContext,
            IMemoryCache memoryCache,
            IConfiguration configuration,
            IEventAggregator eventAggregator,
            IElevatorVolatileDataProvider elevatorVolatileDataProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ILogger<ElevatorDataProvider> logger)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.elevatorVolatileDataProvider = elevatorVolatileDataProvider ?? throw new ArgumentNullException(nameof(elevatorVolatileDataProvider));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));

            this.cacheOptions = configuration.GetMemoryCacheOptions();
        }

        #endregion

        #region Properties

        public double HorizontalPosition
        {
            get => this.elevatorVolatileDataProvider.HorizontalPosition;
            set
            {
                if (this.elevatorVolatileDataProvider.HorizontalPosition != value)
                {
                    this.elevatorVolatileDataProvider.HorizontalPosition = value;

                    this.NotifyElevatorPositionChanged();
                }
            }
        }

        public double VerticalPosition
        {
            get => this.elevatorVolatileDataProvider.VerticalPosition;
            set
            {
                if (this.elevatorVolatileDataProvider.VerticalPosition != value)
                {
                    this.elevatorVolatileDataProvider.VerticalPosition = value;

                    var currentCell = this.GetCurrentCell();
                    if (currentCell != null && !this.IsVerticalPositionWithinTolerance(currentCell.Position))
                    {
                        this.logger.LogDebug($"Elevator is leaving cell id={currentCell.Id}");
                        this.SetCurrentCell(null);
                    }

                    var currentBayPosition = this.GetCurrentBayPosition();
                    if (currentBayPosition != null && !this.IsVerticalPositionWithinTolerance(currentBayPosition.Height))
                    {
                        this.logger.LogDebug($"Elevator is leaving bay position id={currentBayPosition.Id}");
                        this.SetCurrentBayPosition(null);
                    }

                    this.NotifyElevatorPositionChanged();
                }
            }
        }

        #endregion

        #region Methods

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

        public IDbContextTransaction GetContextTransaction()
        {
            return this.dataContext.Database.BeginTransaction();
        }

        public BayPosition GetCurrentBayPosition()
        {
            lock (this.dataContext)
            {
                // if (!this.cache.TryGetValue(ElevatorCurrentBayPositionCacheKey, out BayPosition currentBayPosition))
                //{
                var currentBayPosition = this.dataContext.Elevators
                    .Select(e => e.BayPosition)
                    .Include(p => p.LoadingUnit)
                    .SingleOrDefault();

                this.cache.Set(ElevatorCurrentBayPositionCacheKey, currentBayPosition, this.cacheOptions);
                // }

                return currentBayPosition;
            }
        }

        public Cell GetCurrentCell()
        {
            lock (this.dataContext)
            {
                //    if (!this.cache.TryGetValue(ElevatorCurrentCellCacheKey, out Cell currentCell))
                //    {
                var currentCell = this.dataContext.Elevators
                    .Select(e => e.Cell)
                    .Include(c => c.LoadingUnit)
                    .SingleOrDefault();

                this.cache.Set(ElevatorCurrentCellCacheKey, currentCell, this.cacheOptions);
                //    }

                return currentCell;
            }
        }

        public ElevatorAxis GetHorizontalAxis() => this.GetAxis(Orientation.Horizontal);

        public LoadingUnit GetLoadingUnitOnBoard()
        {
            lock (this.dataContext)
            {
                var elevator = this.dataContext.Elevators.AsNoTracking()
                    .Include(e => e.LoadingUnit)
                    .ThenInclude(l => l.Cell)
                    .Single();

                return elevator.LoadingUnit;
            }
        }

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

        public ElevatorAxis GetVerticalAxis() => this.GetAxis(Orientation.Vertical);

        public void IncreaseCycleQuantity(Orientation orientation)
        {
            lock (this.dataContext)
            {
                var axis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == orientation);
                if (axis is null)
                {
                    throw new EntityNotFoundException(orientation.ToString());
                }

                axis.TotalCycles++;

                this.dataContext.SaveChanges();
            }
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

                elevator.LoadingUnit = null;
                elevator.LoadingUnitId = null;

                this.dataContext.SaveChanges();

                this.cache.Remove(GetAxisCacheKey(Orientation.Horizontal));
                this.cache.Remove(GetAxisCacheKey(Orientation.Vertical));
            }
        }

        public MovementParameters ScaleMovementsByWeight(Orientation orientation)
        {
            var axis = orientation == Orientation.Horizontal
                ? this.GetHorizontalAxis()
                : this.GetVerticalAxis();

            var loadingUnit = this.GetLoadingUnitOnBoard();

            return axis.ScaleMovementsByWeight(loadingUnit);
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
                    }
                    else
                    {
                        elevator.BayPosition = null;
                    }

                    this.dataContext.SaveChanges();

                    this.cache.Remove(ElevatorCurrentBayPositionCacheKey);

                    this.NotifyElevatorPositionChanged();
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
                    .Single();

                if (currentCell?.Id != cellId)
                {
                    if (cellId.HasValue)
                    {
                        var newCell = this.dataContext.Cells.SingleOrDefault(c => c.Id == cellId);
                        if (newCell is null)
                        {
                            throw new EntityNotFoundException(cellId.Value);
                        }

                        elevator.Cell = newCell;
                    }
                    else
                    {
                        elevator.Cell = null;
                    }

                    this.dataContext.SaveChanges();

                    this.cache.Remove(ElevatorCurrentCellCacheKey);

                    this.NotifyElevatorPositionChanged();
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

        public void UpdateVerticalResolution(decimal newResolution)
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

        internal static string GetAxisCacheKey(Orientation orientation) => $"{nameof(GetAxis)}{orientation}";

        private bool IsVerticalPositionWithinTolerance(double position)
        {
            return
                this.elevatorVolatileDataProvider.VerticalPosition - VerticalPositionValidationTolerance < position
                &&
                this.elevatorVolatileDataProvider.VerticalPosition + VerticalPositionValidationTolerance > position;
        }

        private void NotifyElevatorPositionChanged()
        {
            this.eventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(
                    new NotificationMessage
                    {
                        Data = new ElevatorPositionMessageData(
                            this.elevatorVolatileDataProvider.VerticalPosition,
                            this.elevatorVolatileDataProvider.HorizontalPosition,
                            this.GetCurrentCell()?.Id,
                            this.GetCurrentBayPosition()?.Id),
                        Destination = MessageActor.Any,
                        Source = MessageActor.DataLayer,
                        Type = MessageType.ElevatorPosition,
                    });
        }

        #endregion
    }
}
