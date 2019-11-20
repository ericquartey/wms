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

        #endregion

        #region Constructors

        public LoadingUnitsProvider(DataLayerContext dataContext)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
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
                var loadingUnit = this.dataContext.LoadingUnits.FirstOrDefault(l => l.Id == id);
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

                if (loadingUnitGrossWeight < MinimumLoadOnBoard + elevator.StructuralProperties.ElevatorWeight)
                {
                    throw new ArgumentOutOfRangeException(
                        $"The loading unit's weight ({loadingUnitGrossWeight}kg) is lower than the expected minimum weight ({MinimumLoadOnBoard}kg).");
                }

                var loadingUnit = this.dataContext
                    .LoadingUnits
                    .SingleOrDefault(l => l.Id == loadingUnitId);

                if (loadingUnitGrossWeight > loadingUnit.MaxNetWeight + loadingUnit.Tare + elevator.StructuralProperties.ElevatorWeight)
                {
                    throw new ArgumentOutOfRangeException(
                        $"The specified gross weight ({loadingUnitGrossWeight}) is greater than the loading unit's weight capacity (max net: {loadingUnit.MaxNetWeight}, tare: {loadingUnit.Tare}).");
                }

                loadingUnit.GrossWeight = loadingUnitGrossWeight - elevator.StructuralProperties.ElevatorWeight;

                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
