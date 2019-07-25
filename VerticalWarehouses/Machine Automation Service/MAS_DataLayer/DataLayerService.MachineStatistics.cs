using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IMachineStatisticsDataLayer
    {
        #region Methods

        public MachineStatistics GetMachineStatistics()
        {
            using (var primaryDataContext = new DataLayerContext(this.primaryContextOptions))
            {
                return primaryDataContext.MachineStatistics.FirstOrDefault();
            }
        }

        #endregion
    }
}
