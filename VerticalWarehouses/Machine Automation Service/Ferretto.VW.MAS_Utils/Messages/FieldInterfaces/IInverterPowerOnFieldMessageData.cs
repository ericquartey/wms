using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface IInverterPowerOnFieldMessageData : IFieldMessageData
    {
        #region Properties

        InverterIndex InverterToPowerOn { get; set; }

        #endregion
    }
}
