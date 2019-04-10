using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;
using Axis = Ferretto.VW.MAS_Utils.Enumerations.Axis;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_Utils.Messages.Data
{
    public class SwitchAxisMessageData : ISwitchAxisMessageData
    {
        #region Constructors

        public SwitchAxisMessageData(Axis axisToSwitch, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisToSwitch = axisToSwitch;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public Axis AxisToSwitch { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
