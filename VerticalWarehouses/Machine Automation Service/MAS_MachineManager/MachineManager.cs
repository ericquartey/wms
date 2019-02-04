using System.Threading.Tasks;
using Ferretto.Common.Common_Utils;
using Ferretto.VW.MAS_FiniteStateMachines;

namespace Ferretto.VW.MAS_MachineManager
{
    public class MachineManager : IMachineManager
    {
        #region Fields

        private readonly FiniteStateMachines finiteStateMachines = Singleton<FiniteStateMachines>.UniqueInstance;

        #endregion Fields

        #region Methods

        public void DoHoming(BroadcastDelegate broadcastDelegate)
        {
            this.finiteStateMachines.DoHoming(broadcastDelegate);
        }

        #endregion Methods
    }
}
