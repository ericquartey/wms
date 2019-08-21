using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IMachineStatisticsProvider
    {
        #region Methods

        MachineStatistics GetMachineStatistics();

        #endregion
    }
}
