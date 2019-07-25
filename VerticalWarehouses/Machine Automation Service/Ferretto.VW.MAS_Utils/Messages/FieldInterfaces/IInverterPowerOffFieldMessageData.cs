using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterPowerOffFieldMessageData : IFieldMessageData
    {
        #region Properties

        InverterIndex InverterToPowerOff { get; set; }

        #endregion
    }
}
