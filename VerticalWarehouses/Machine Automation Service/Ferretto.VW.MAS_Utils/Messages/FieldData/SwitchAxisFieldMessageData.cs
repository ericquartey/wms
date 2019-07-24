using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class SwitchAxisFieldMessageData : ISwitchAxisFieldMessageData
    {
        #region Constructors

        public SwitchAxisFieldMessageData(Axis axisToSwitchOn, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisToSwitchOn = axisToSwitchOn;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public Axis AxisToSwitchOn { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"AxisToSwitch:{this.AxisToSwitchOn}";
        }

        #endregion
    }
}
