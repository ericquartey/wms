using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterSetTimerFieldMessageData : IFieldMessageData
    {
        #region Properties

        bool Enable { get; }

        InverterIndex inverterIndex { get; set; }

        InverterTimer InverterTimer { get; }

        int UpdateInterval { get; }

        #endregion
    }
}
