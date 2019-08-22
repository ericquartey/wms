namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterPowerOnFieldMessageData : IFieldMessageData
    {


        #region Properties

        FieldCommandMessage NextCommandMessage { get; set; }

        #endregion
    }
}
