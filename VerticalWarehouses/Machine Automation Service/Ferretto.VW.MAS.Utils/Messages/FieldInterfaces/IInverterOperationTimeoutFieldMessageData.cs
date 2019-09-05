namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterOperationTimeoutFieldMessageData : IFieldMessageData
    {
        #region Properties

        ushort ControlWord { get; }

        #endregion
    }
}
