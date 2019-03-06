using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    internal class SwitchAxisMessageData : ISwitchAxisMessageData
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
