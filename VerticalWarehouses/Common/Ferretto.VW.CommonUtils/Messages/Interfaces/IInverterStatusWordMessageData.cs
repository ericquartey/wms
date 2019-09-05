namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IInverterStatusWordMessageData : IMessageData
    {
        #region Properties

        byte InverterIndex { get; }
        ushort Value { get; }

        #endregion
    }
}
