namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IStateChangedMessageData : IMessageData
    {


        #region Properties

        bool CurrentState { get; }

        #endregion
    }
}
