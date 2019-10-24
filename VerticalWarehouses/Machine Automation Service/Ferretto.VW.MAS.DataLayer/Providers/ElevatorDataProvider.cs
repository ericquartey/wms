﻿using System;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class ElevatorDataProvider : IElevatorDataProvider
    {
        #region Fields

        private static readonly MemoryCacheEntryOptions CacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(1));

        private readonly IMemoryCache cache;

        private readonly DataLayerContext dataContext;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        #endregion

        #region Constructors

        public ElevatorDataProvider(
            DataLayerContext dataContext,
            IMemoryCache memoryCache,
            ISetupProceduresDataProvider setupProceduresDataProvider)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
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
                    cacheEntry = this.dataContext.ElevatorAxes
                        .Include(a => a.Profiles)
                        .ThenInclude(p => p.Steps)
                        .Include(a => a.FullLoadMovement)
                        .Include(a => a.EmptyLoadMovement)
                        .SingleOrDefault(a => a.Orientation == orientation);

                    if (cacheEntry is null)
                    {
                        throw new EntityNotFoundException(orientation.ToString());
                    }

                    this.cache.Set(cacheKey, cacheEntry, CacheOptions);
                }

                return cacheEntry;
            }
        }

        public IDbContextTransaction GetContextTransaction()
        {
            return this.dataContext.Database.BeginTransaction();
        }

        public ElevatorAxis GetHorizontalAxis() => this.GetAxis(Orientation.Horizontal);

        public LoadingUnit GetLoadingUnitOnBoard()
        {
            lock (this.dataContext)
            {
                var elevator = this.dataContext.Elevators
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

        public void UnloadLoadingUnit()
        {
            lock (this.dataContext)
            {
                var elevator = this.dataContext
                    .Elevators
                    .Include(e => e.LoadingUnit)
                    .Single();

                elevator.LoadingUnitId = null;

                this.dataContext.SaveChanges();
            }
        }

        public void UpdateVerticalOffset(double newOffset)
        {
            lock (this.dataContext)
            {
                var verticalAxis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == Orientation.Vertical);

                var cacheKey = GetAxisCacheKey(Orientation.Vertical);
                this.cache.Set(cacheKey, verticalAxis, CacheOptions);

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
                var verticalAxis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == Orientation.Vertical);

                var cacheKey = GetAxisCacheKey(Orientation.Vertical);
                this.cache.Set(cacheKey, verticalAxis, CacheOptions);

                verticalAxis.Resolution = newResolution;
                this.dataContext.ElevatorAxes.Update(verticalAxis);
                this.dataContext.SaveChanges();

                var procedureParameters = this.setupProceduresDataProvider.GetVerticalResolutionCalibration();
                this.setupProceduresDataProvider.MarkAsCompleted(procedureParameters);
            }
        }

        private static string GetAxisCacheKey(Orientation orientation) => $"{nameof(GetAxis)}{orientation}";

        #endregion
    }
}
