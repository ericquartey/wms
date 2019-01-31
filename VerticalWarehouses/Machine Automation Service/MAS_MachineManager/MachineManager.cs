using System.Threading.Tasks;
using Ferretto.Common.Common_Utils;

namespace Ferretto.VW.MAS_MachineManager
{
    public class MachineManager : IMachineManager
    {
        #region Fields

        private readonly FiniteStateMachines finiteStateMachines = Singleton<FiniteStateMachines>.UniqueInstance;

        #endregion Fields

        #region Methods

        public async Task DoHoming(BroadcastDelegate _delegate)
        {
            await this.finiteStateMachines.DoHoming(_delegate);
        }

        #endregion Methods
    }
}
