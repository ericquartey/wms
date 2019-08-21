using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterPowerOnFieldMessageData : IFieldMessageData
    {
        #region Properties

        InverterIndex InverterToPowerOn { get; set; }

        #endregion
    }
}
