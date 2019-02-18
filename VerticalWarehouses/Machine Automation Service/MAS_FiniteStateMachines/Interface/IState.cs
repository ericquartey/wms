using Ferretto.VW.Common_Utils.Messages;

namespace Ferretto.VW.MAS_FiniteStateMachines
{
    public interface IState
    {
        #region Properties

        string Type { get; }

        #endregion

        #region Methods

        void NotifyMessage( Event_Message message );

        #endregion
    }
}
