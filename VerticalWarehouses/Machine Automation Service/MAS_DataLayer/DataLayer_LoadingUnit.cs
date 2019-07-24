using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.LoadingUnit;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : ILoadingUnitStatistics
    {
        #region Methods

        public IEnumerable<LoadingUnitSpaceStatistics> GetSpaceStatistics()
        {
            using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
            {
                var loadingUnits = primaryDataContext.LoadingUnits.Select(l =>
                     new LoadingUnitSpaceStatistics
                     {
                         MissionsCount = l.MissionsCount,
                         Code = l.Code,
                     }
                );

                return loadingUnits.ToArray();
            }
        }

        public IEnumerable<LoadingUnitWeightStatistics> GetWeightStatistics()
        {
            using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
            {
                var loadingUnits = primaryDataContext.LoadingUnits.Select(l =>
                     new LoadingUnitWeightStatistics
                     {
                         Height = l.Height,
                         GrossWeight = l.GrossWeight,
                         Tare = l.Tare,
                         Code = l.Code,
                         MaxNetWeight = l.MaxNetWeight,
                         MaxWeightPercentage = (l.GrossWeight - l.Tare) * 100 / l.MaxNetWeight,
                     }
                );

                return loadingUnits.ToArray();
            }
        }

        #endregion
    }
}
