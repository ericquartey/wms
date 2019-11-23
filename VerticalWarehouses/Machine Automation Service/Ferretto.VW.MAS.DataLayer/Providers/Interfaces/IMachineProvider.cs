using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IMachineProvider
    {
        #region Properties

        bool IsMachineRunning { get; set; }

        #endregion

        #region Methods

        void Add(Machine machine);

        void ClearAll();

        Machine Get();

        double GetHeight();

        MachineStatistics GetStatistics();

        bool IsOneTonMachine();

        void Update(Machine machine);

        #endregion
    }
}
