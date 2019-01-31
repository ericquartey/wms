using System.Threading.Tasks;
using Ferretto.Common.Common_Utils;

namespace Ferretto.VW.MAS_MachineManager
{
    public class MachineManager : IMachineManager
    {
        //TODO private readonly FiniteStateMachines finiteStateMachines = Singleton<FiniteStateMachines>.UniqueInstance;

        #region Methods

        public async Task DoHoming(BroadcastDelegate broadcastDelegate)
        {
            //TODO await this.finiteStateMachines.DoHoming(broadcastDelegate);
        }

        #endregion Methods
    }
}
