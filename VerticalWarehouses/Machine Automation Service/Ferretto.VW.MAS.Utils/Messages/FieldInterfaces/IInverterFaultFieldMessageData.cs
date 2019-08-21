using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterFaultFieldMessageData : IFieldMessageData
    {
        #region Properties

        InverterIndex InverterToReset { get; set; }

        #endregion
    }
}
