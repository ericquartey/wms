using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterSetTimerFieldMessageData : IFieldMessageData
    {
        #region Properties

        bool Enable { get; }

        InverterTimer InverterTimer { get; }

        int UpdateInterval { get; }

        #endregion
    }
}
