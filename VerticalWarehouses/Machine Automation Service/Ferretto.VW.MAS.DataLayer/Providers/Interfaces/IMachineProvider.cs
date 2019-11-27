using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ferretto.VW.MAS.DataLayer
{
    public interface IMachineProvider
    {
        #region Properties

        bool IsHomingExecuted { get; set; }

        bool IsMachineRunning { get; set; }

        #endregion

        #region Methods

        void Add(Machine machine);

        void ClearAll();

        Machine Get();

        double GetHeight();

        int GetIdentity();

        MachineStatistics GetStatistics();

        void Import(Machine machine, DataLayerContext context);

        bool IsOneTonMachine();

        void Update(Machine machine, DataLayerContext context);

        #endregion
    }
}
