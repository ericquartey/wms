namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterPowerOffFieldMessageData : IFieldMessageData
    {
        #region Properties

        FieldCommandMessage NextCommandMessage { get; set; }

        #endregion
    }
}
