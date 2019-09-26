namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IChangeRunningStateMessageData : IMessageData
    {


        #region Properties

        CommandAction CommandAction { get; }

        bool Enable { get; }

        #endregion
    }
}
