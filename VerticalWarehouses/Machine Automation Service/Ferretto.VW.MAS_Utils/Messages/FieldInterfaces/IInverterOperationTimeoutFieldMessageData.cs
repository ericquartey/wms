namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface IInverterOperationTimeoutFieldMessageData : IFieldMessageData
    {
        #region Properties

        ushort ControlWord { get; }

        #endregion
    }
}
