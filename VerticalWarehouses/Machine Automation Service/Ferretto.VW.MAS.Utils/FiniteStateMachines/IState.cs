using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.Utils
{
    public interface IState
    {
        #region Methods

        void Enter(IMessageData data);

        void Exit();

        #endregion
    }
}
