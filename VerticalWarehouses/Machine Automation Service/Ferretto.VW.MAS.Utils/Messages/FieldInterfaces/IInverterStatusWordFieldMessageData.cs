namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterStatusWordFieldMessageData : IFieldMessageData
    {
        #region Properties

        ushort Value { get; }

        #endregion
    }
}
