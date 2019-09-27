using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterSetTimerFieldMessageData : IFieldMessageData
    {
        #region Properties

        bool Enable { get; }

        InverterIndex InverterIndex { get; set; }

        InverterTimer InverterTimer { get; }

        int UpdateInterval { get; }

        #endregion
    }
}
