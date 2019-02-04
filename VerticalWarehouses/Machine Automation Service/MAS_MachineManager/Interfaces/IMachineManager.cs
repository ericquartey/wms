using Ferretto.Common.Common_Utils;

namespace Ferretto.VW.MAS_MachineManager
{
    public interface IMachineManager
    {
        #region Methods

        void DoHoming(BroadcastDelegate broadcastDelegate);

        #endregion Methods
    }
}
