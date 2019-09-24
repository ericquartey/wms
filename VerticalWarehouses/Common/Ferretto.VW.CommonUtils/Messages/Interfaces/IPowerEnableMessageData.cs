namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IPowerEnableMessageData : IMessageData
    {


        #region Properties

        CommandAction CommandAction { get; }

        bool Enable { get; }

        #endregion
    }
}
