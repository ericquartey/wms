using Ferretto.Common.Common_Utils;
using Ferretto.VW.MAS_FiniteStateMachines;

namespace Ferretto.VW.MAS_MachineManager
{
    public class MachineManager : IMachineManager
    {
        #region Fields

        private readonly FiniteStateMachines finiteStateMachines;

        #endregion Fields

        #region Constructors

        public MachineManager()
        {
            //this.finiteStateMachines = Singleton<FiniteStateMachines>.UniqueInstance;
        }

        #endregion Constructors

        #region Methods

        public void Destroy()
        {
            this.finiteStateMachines.Destroy();
        }

        public void DoHoming(BroadcastDelegate broadcastDelegate)
        {
            this.finiteStateMachines.DoHoming(broadcastDelegate);
        }

        public void DoVerticalHoming(BroadcastDelegate broadcastDelegate)
        {
            this.finiteStateMachines.DoVerticalHoming(broadcastDelegate);
        }

        #endregion Methods
    }
}
