using Ferretto.Common.Common_Utils;
using Ferretto.VW.MAS_FiniteStateMachines;
using MAS_DataLayer;

namespace Ferretto.VW.MAS_MachineManager
{
    public class MachineManager : IMachineManager
    {
        #region Fields

        private readonly IFiniteStateMachines finiteStateMachines;

        private readonly IWriteLogService writeLogService;

        #endregion Fields

        #region Constructors

        public MachineManager(IFiniteStateMachines finiteStateMachines, IWriteLogService writeLogService)
        {
            this.finiteStateMachines = finiteStateMachines;
            this.writeLogService = writeLogService;
        }

        #endregion Constructors

        #region Methods

        public void DoHoming()
        {
            this.finiteStateMachines.DoHoming();
        }

        #endregion Methods
    }
}
