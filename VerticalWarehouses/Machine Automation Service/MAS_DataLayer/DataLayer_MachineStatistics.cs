using System.Linq;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IMachineStatisticsDataLayer
    {
        public MachineStatistics GetMachineStatistics()
        {
            return this.primaryDataContext.MachineStatistics.FirstOrDefault();
        }
    }
}
