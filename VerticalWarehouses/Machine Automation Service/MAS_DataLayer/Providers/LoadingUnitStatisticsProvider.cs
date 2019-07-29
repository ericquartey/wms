using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels.LoadingUnits;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal class LoadingUnitStatisticsProvider : ILoadingUnitStatisticsProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public LoadingUnitStatisticsProvider(DataLayerContext dataContext)
        {
            if (dataContext == null)
            {
                throw new System.ArgumentNullException(nameof(dataContext));
            }

            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics()
        {
            var loadingUnits = this.dataContext.LoadingUnits.Select(l =>
                 new LoadingUnitSpaceStatistics
                 {
                     MissionsCount = l.MissionsCount,
                     Code = l.Code,
                 }
            ).ToArray();

            return loadingUnits;
        }

        public IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics()
        {
            var loadingUnits = this.dataContext.LoadingUnits.Select(l =>
                 new LoadingUnitWeightStatistics
                 {
                     Height = l.Height,
                     GrossWeight = l.GrossWeight,
                     Tare = l.Tare,
                     Code = l.Code,
                     MaxNetWeight = l.MaxNetWeight,
                     MaxWeightPercentage = (l.GrossWeight - l.Tare) * 100 / l.MaxNetWeight,
                 }
            ).ToArray();

            return loadingUnits;
        }

        #endregion
    }
}
