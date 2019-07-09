using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface ISwitchAxisFieldMessageData : IFieldMessageData
    {
        #region Properties

        Axis AxisToSwitchOn { get; }

        #endregion
    }
}
