using System.Linq;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : IMachineStatisticsDataLayer
    {
        public MachineStatistics GetMachineStatistics()
        {
            return this.primaryDataContext.MachineStatistics.FirstOrDefault();
        }
    }
}
