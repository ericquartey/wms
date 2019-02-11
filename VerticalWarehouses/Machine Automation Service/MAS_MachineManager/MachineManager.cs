using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines;

namespace Ferretto.VW.MAS_MachineManager
{
    public class MachineManager : IMachineManager
    {
        #region Fields

        private readonly IFiniteStateMachines finiteStateMachines;

        private readonly IWriteLogService writeLogService;

        #endregion

        #region Constructors

        public MachineManager(IFiniteStateMachines finiteStateMachines, IWriteLogService writeLogService)
        {
            this.finiteStateMachines = finiteStateMachines;
            this.writeLogService = writeLogService;
        }

        #endregion
    }
}
