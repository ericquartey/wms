using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IMachineProvider
    {
        #region Methods

        Machine Get();

        double GetHeight();

        MachineStatistics GetStatistics();

        bool IsOneTonMachine();

        void Update(Machine machine);

        #endregion
    }
}
