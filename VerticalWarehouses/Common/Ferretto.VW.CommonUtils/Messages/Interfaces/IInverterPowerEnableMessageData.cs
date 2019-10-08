namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IInverterPowerEnableMessageData : IMessageData
    {
        #region Properties

        bool Enable { get; }

        #endregion
    }
}
