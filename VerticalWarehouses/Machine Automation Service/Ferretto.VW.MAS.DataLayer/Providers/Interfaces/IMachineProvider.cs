using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IMachineProvider
    {
        #region Methods

        void Add(Machine machine);

        void ClearAll();

        Machine Get();

        double GetHeight();

        MachineStatistics GetStatistics();

        void Import(Machine machine);

        bool IsOneTonMachine();

        void Update(Machine machine);

        #endregion
    }
}
