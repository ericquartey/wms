using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_FiniteStateMachines.Interface
{
    public interface IHomingStateMachine
    {
        #region Properties

        ICalibrateMessageData CalibrateData { get; }

        #endregion
    }
}
