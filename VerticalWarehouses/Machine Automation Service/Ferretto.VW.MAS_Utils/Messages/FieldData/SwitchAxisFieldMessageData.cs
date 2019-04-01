using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class SwitchAxisFieldMessageData : ISwitchAxisFieldMessageData
    {
        #region Constructors

        public SwitchAxisFieldMessageData(Axis axisToSwitchOn, Axis axisToSwitchOff, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisToSwitchOn = axisToSwitchOn;
            this.AxisToSwitchOff = axisToSwitchOff;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public Axis AxisToSwitchOff { get; }

        public Axis AxisToSwitchOn { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
