using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface IInverterSwitchOnFieldMessageData : IFieldMessageData
    {
        #region Properties

        Axis AxisToSwitchOn { get; }

        InverterIndex SystemIndex { get; }

        #endregion
    }
}
