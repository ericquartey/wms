using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IMachineStatisticsDataLayer
    {
        MachineStatistics GetMachineStatistics();
    }
}
