using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class LoadingUnitsDataProvider : ILoadingUnitsDataProvider
    {
        #region Fields

        /// <summary>
        /// TODO Consider transformomg this constant in configuration parameters.
        /// </summary>
        private const double MinimumLoadOnBoard = 10.0;

        private readonly DataLayerContext dataContext;

        private readonly ILogger<LoadingUnitsDataProvider> logger;

        private readonly IMachineProvider machineProvider;

        #endregion

        #region Constructors

        public LoadingUnitsDataProvider(
            DataLayerContext dataContext,
            IMachineProvider machineProvider,
            ILogger<LoadingUnitsDataProvider> logger)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.machineProvider = machineProvider ?? throw new System.ArgumentNullException(nameof(machineProvider));
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

        public MachineErrorCode CheckWeight(int id)
        {
            var check = MachineErrorCode.NoError;
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext
                    .LoadingUnits
                    .SingleOrDefault(l => l.Id == id);

                if (loadingUnit is null)
                {
                    throw new EntityNotFoundException(id);
                }

                var machine = this.machineProvider.Get();

                if (loadingUnit.GrossWeight < MinimumLoadOnBoard)
                {
                    check = MachineErrorCode.LoadUnitWeightTooLow;
                }
                else if (loadingUnit.GrossWeight > loadingUnit.MaxNetWeight + loadingUnit.Tare)
                {
                    check = MachineErrorCode.LoadUnitWeightExceeded;
                }
                else
                {
                    var totalWeight = this.dataContext
                        .LoadingUnits
                        .Where(lu => lu.IsIntoMachine)
                        .Sum(lu => lu.GrossWeight);
                    if (loadingUnit.GrossWeight + totalWeight > machine.MaxGrossWeight)
                    {
                        check = MachineErrorCode.MachineWeightExceeded;
                    }
                }
            }
            return check;
        }

        public IEnumerable<LoadingUnit> GetAll()
        {
            lock (this.dataContext)
            {
                return this.dataContext.LoadingUnits
                    .AsNoTracking()
                    .Include(l => l.Cell)
                    .ThenInclude(c => c.Panel)
                    .ToArray();
            }
        }

        public LoadingUnit GetByBayId(int bayId)
        {
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext.BayPositions.Include(i => i.LoadingUnit).AsNoTracking()
                    .FirstOrDefault().LoadingUnit;
                if (loadingUnit is null)
                {
                    throw new EntityNotFoundException(bayId);
                }

                return loadingUnit;
            }
        }

        public LoadingUnit GetByCellId(int cellId)
        {
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext.LoadingUnits.AsNoTracking()
                    .FirstOrDefault(l => l.CellId == cellId);
                if (loadingUnit is null)
                {
                    throw new EntityNotFoundException(cellId);
                }

                return loadingUnit;
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

        public void Import(IEnumerable<LoadingUnit> loadingUnits, DataLayerContext context)
        {
            _ = loadingUnits ?? throw new ArgumentNullException(nameof(loadingUnits));

            context.Delete(loadingUnits, (e) => e.Id);
            loadingUnits.ForEach((l) => context.AddOrUpdate(l, (e) => e.Id));

            this.machineProvider.UpdateWeightStatistics(context);
        }

        public void Insert(int loadingUnitsId)
        {
            var machine = this.machineProvider.Get();
            lock (this.dataContext)
            {
                var loadingUnits = new LoadingUnit
                {
                    Id = loadingUnitsId,
                    Tare = machine.LoadUnitTare,
                    MaxNetWeight = machine.LoadUnitMaxNetWeight,
                    Height = 0
                };

                this.dataContext.LoadingUnits.Add(loadingUnits);

                this.dataContext.SaveChanges();
            }
        }

        public void SetHeight(int id, double height)
        {
            lock (this.dataContext)
            {
                var loadingUnit = this.dataContext
                    .LoadingUnits
                    .SingleOrDefault(l => l.Id == id);

                if (loadingUnit is null)
                {
                    throw new EntityNotFoundException(id);
                }
                if (height == 0)
                {
                    loadingUnit.Height = 0;
                }
                else
                {
                    loadingUnit.Height = Math.Max(loadingUnit.Height, height);
                }
                this.dataContext.SaveChanges();
            }
        }

        public void SetWeight(int id, double loadingUnitGrossWeight)
        {
            lock (this.dataContext)
            {
                var elevator = this.dataContext.Elevators
                    .Include(e => e.StructuralProperties)
                    .Single();

                var elevatorWeight = elevator.StructuralProperties.ElevatorWeight;

                var loadingUnit = this.dataContext
                    .LoadingUnits
                    .SingleOrDefault(l => l.Id == id);

                loadingUnit.GrossWeight = loadingUnitGrossWeight - elevatorWeight;

                this.dataContext.SaveChanges();
            }
        }

        public void UpdateRange(IEnumerable<LoadingUnit> loadingUnits, DataLayerContext dataContext)
        {
            _ = loadingUnits ?? throw new ArgumentNullException(nameof(loadingUnits));

            if (dataContext is null)
            {
                dataContext = this.dataContext;
            }

            loadingUnits.ForEach((l) => dataContext.AddOrUpdate(l, (e) => e.Id));

            dataContext.SaveChanges();
        }

        public void UpdateWeightStatistics()
        {
            lock (this.dataContext)
            {
                this.machineProvider.UpdateWeightStatistics(this.dataContext);
            }
        }

        #endregion
    }
}
