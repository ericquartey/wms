using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IMachineProvider
    {
        #region Methods

        Machine Get();

        MachineStatistics GetStatistics();

        bool IsOneTonMachine();

        #endregion
    }
}
