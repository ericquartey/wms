using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface IInverterPowerOffFieldMessageData : IFieldMessageData
    {
        #region Properties

        InverterIndex InverterToPowerOff { get; set; }

        #endregion
    }
}
