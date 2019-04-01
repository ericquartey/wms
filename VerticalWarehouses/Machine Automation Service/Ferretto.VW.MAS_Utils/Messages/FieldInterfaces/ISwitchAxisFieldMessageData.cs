using Ferretto.VW.Common_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface ISwitchAxisFieldMessageData : IFieldMessageData
    {
        #region Properties

        Axis AxisToSwitchOff { get; }

        Axis AxisToSwitchOn { get; }

        #endregion
    }
}
