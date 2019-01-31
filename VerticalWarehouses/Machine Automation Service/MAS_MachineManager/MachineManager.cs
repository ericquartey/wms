using System.Threading.Tasks;
using Ferretto.VW.Common_Utils;
using System;

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
