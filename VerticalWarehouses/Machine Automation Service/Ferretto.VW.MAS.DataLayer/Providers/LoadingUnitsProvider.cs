﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class LoadingUnitsProvider : ILoadingUnitsProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly ILogger<LoadingUnitsProvider> logger;

        #endregion

        #region Constructors

        public LoadingUnitsProvider(DataLayerContext dataContext, ILogger<LoadingUnitsProvider> logger)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public IEnumerable<LoadingUnit> GetAll()
        {
            lock (this.dataContext)
            {
                return this.dataContext.LoadingUnits.ToArray();
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
                var loadingUnits = this.dataContext.LoadingUnits.Select(l =>
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
                var loadingUnit = this.dataContext
                    .LoadingUnits
                    .SingleOrDefault(l => l.Id == loadingUnitId);

                loadingUnit.GrossWeight = loadingUnitGrossWeight;

                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
