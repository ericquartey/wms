using Ferretto.Common.Common_Utils;

namespace Ferretto.VW.MAS_MachineManager
{
    public interface IMachineManager
    {
        #region Methods

        void Destroy();

        /// <summary>
        /// Execute the complete homing operation.
        /// </summary>
        void DoHoming(BroadcastDelegate broadcastDelegate);

        /// <summary>
        /// Execute the vertical homing operation.
        /// </summary>
        void DoVerticalHoming(BroadcastDelegate broadcastDelegate);

        #endregion Methods
    }
}
