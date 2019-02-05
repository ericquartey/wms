using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines;

namespace Ferretto.VW.MAS_MachineManager
{
    public class MachineManager : IMachineManager
    {
        #region Fields

        private readonly IFiniteStateMachines finiteStateMachines;

        private readonly IWriteLogService writeLogService;

        private int value;

        #endregion

        #region Constructors

        public MachineManager(IFiniteStateMachines finiteStateMachines, IWriteLogService writeLogService)
        {
            this.finiteStateMachines = finiteStateMachines;
            this.writeLogService = writeLogService;

            this.value = -1;
        }

        #endregion

        #region Methods

        public void DoHoming()
        {
            this.finiteStateMachines.DoHoming();
        }

        #endregion
    }
}
