using Ferretto.Common.Common_Utils;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public interface IFiniteStateMachines
    {
        #region Methods

        void Destroy();

        /// <summary>
        /// Execute complete homing
        /// </summary>
        void DoHoming(BroadcastDelegate broadcastDelegate);

        /// <summary>
        /// Execute vertical homing
        /// </summary>
        void DoVerticalHoming(BroadcastDelegate broadcastDelegate);

        #endregion Methods

        /*
        void MakeOperationByInverter(IdOperation code);
        */
    }
}
