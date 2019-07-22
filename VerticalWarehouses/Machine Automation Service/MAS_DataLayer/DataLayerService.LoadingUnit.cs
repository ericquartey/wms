using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataLayer.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : ILoadingUnitStatisticsDataLayer
    {
        #region Methods

        public IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics()
        {
            var loadingUnits = this.primaryDataContext.LoadingUnits.Select(l =>
                 new LoadingUnitSpaceStatistics
                 {
                     MissionsCount = l.MissionsCount,
                     Code = l.Code,
                 }
            );

            return loadingUnits;
        }

        public IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics()
        {
            var loadingUnits = this.primaryDataContext.LoadingUnits.Select(l =>
                 new LoadingUnitWeightStatistics
                 {
                     Height = l.Height,
                     GrossWeight = l.GrossWeight,
                     Tare = l.Tare,
                     Code = l.Code,
                     MaxNetWeight = l.MaxNetWeight,
                     MaxWeightPercentage = (l.GrossWeight - l.Tare) / l.MaxNetWeight,
                 }
            );

            return loadingUnits;
        }

        #endregion
    }
}
