using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class LoadingUnitsProvider : ILoadingUnitsProvider
    {
        #region Fields

        /// <summary>
        /// TODO Consider transformomg this constant in configuration parameters.
        /// </summary>
        private const double MinimumLoadOnBoard = 10.0;

        private readonly DataLayerContext dataContext;

        private readonly ILogger<DataLayerContext> logger;

        #endregion

        #region Constructors

        public LoadingUnitsProvider(
            DataLayerContext dataContext,
            ILogger<DataLayerContext> logger)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public void Add(IEnumerable<LoadingUnit> loadingUnits)
        {
            lock (this.dataContext)
            {
                this.dataContext.LoadingUnits.AddRange(loadingUnits);

                this.dataContext.SaveChanges();
            }
        }

        public void ClearAll()
        {
        }

        public IEnumerable<LoadingUnit> GetAll()
        {
            lock (this.dataContext)
            {
                return this.dataContext.LoadingUnits
                    .Include(i => i.Cell)
                    .ToArray();
            }
        }

        public LoadingUnit GetById(int id)
        {
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext.LoadingUnits.AsNoTracking()
                    .FirstOrDefault(l => l.Id == id);
                if (loadingUnit is null)
                {
                    throw new EntityNotFoundException(id);
                }

                return loadingUnit;
            }
        }

        public IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics()
        {
            lock (this.dataContext)
            {
                var loadingUnits = this.dataContext
                    .LoadingUnits
                    .Select(l =>
                        new LoadingUnitSpaceStatistics
                        {
                            MissionsCount = l.MissionsCount,
                            Code = l.Code,
                        }).ToArray();

                return loadingUnits;
            }
        }

        public IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics()
        {
            lock (this.dataContext)
            {
                var loadingUnits = this.dataContext.LoadingUnits
                .Select(l =>
                     new LoadingUnitWeightStatistics
                     {
                         Height = l.Height,
                         GrossWeight = l.GrossWeight,
                         Tare = l.Tare,
                         Code = l.Code,
                         MaxNetWeight = l.MaxNetWeight,
                         MaxWeightPercentage = (l.GrossWeight - l.Tare) * 100 / l.MaxNetWeight,
                     })
                .ToArray();

                return loadingUnits;
            }
        }

        public void Import(IEnumerable<LoadingUnit> loadingUnits)
        {
            lock (this.dataContext)
            {
                try
                {
                    this.dataContext.Delete(loadingUnits, (e) => e.Id);
                    loadingUnits.ForEach((l) => this.dataContext.AddOrUpdate(l, (e) => e.Id));

                    this.dataContext.SaveChanges();

                    this.logger.LogDebug($"LoadingUnit import count {loadingUnits?.Count() ?? 0} ");
                }
                catch (Exception e)
                {
                    this.logger.LogError(e, $"LoadingUnit import exception");
                    throw;
                }
            }
        }

        public void Insert(int loadingUnitsId)
        {
            lock (this.dataContext)
            {
                LoadingUnit loadingUnits = new LoadingUnit
                {
                    Id = loadingUnitsId,
                    Tare = 120,
                    MaxNetWeight = 800,
                };
                this.dataContext.LoadingUnits.Add(loadingUnits);

                this.dataContext.SaveChanges();
            }
        }

        public void SetHeight(int loadingUnitId, double height)
        {
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext
                    .LoadingUnits
                    .SingleOrDefault(l => l.Id == loadingUnitId);

                loadingUnit.Height = height;

                this.dataContext.SaveChanges();
            }
        }

        public void SetWeight(int loadingUnitId, double loadingUnitGrossWeight)
        {
            lock (this.dataContext)
            {
                var elevator = this.dataContext.Elevators
                    .Include(e => e.StructuralProperties)
                    .Single();

                var elevatorWeight = elevator.StructuralProperties.ElevatorWeight;

                if (loadingUnitGrossWeight < MinimumLoadOnBoard + elevatorWeight)
                {
                    throw new ArgumentOutOfRangeException(
                        $"The loading unit's weight ({loadingUnitGrossWeight}kg) is lower than the expected minimum weight ({MinimumLoadOnBoard + elevatorWeight}kg).");
                }

                var loadingUnit = this.dataContext
                    .LoadingUnits
                    .SingleOrDefault(l => l.Id == loadingUnitId);

                if (loadingUnitGrossWeight > loadingUnit.MaxNetWeight + loadingUnit.Tare + elevatorWeight)
                {
                    throw new ArgumentOutOfRangeException(
                        $"The specified gross weight ({loadingUnitGrossWeight}) is greater than the loading unit's weight capacity (max net: {loadingUnit.MaxNetWeight}, tare: {loadingUnit.Tare}).");
                }

                loadingUnit.GrossWeight = loadingUnitGrossWeight - elevatorWeight;

                this.dataContext.SaveChanges();
            }
        }

        public void UpdateRange(IEnumerable<LoadingUnit> loadingUnits)
        {
            lock (this.dataContext)
            {
                loadingUnits.ForEach((l) => this.dataContext.AddOrUpdate(l, (e) => e.Id));

                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
