namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IPowerEnableMessageData : IMessageData
    {
        #region Properties

        bool Enable { get; }

        #endregion
    }
}
