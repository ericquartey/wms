using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class CalibrateAxisFieldMessageData : ISwitchAxisFieldMessageData
    {
        #region Constructors

        public CalibrateAxisFieldMessageData(Axis axisToSwitchOn, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisToSwitchOn = axisToSwitchOn;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public Axis AxisToSwitchOn { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
