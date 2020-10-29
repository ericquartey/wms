using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class SwitchAxisMessageData : ISwitchAxisMessageData
    {
        #region Constructors

        public SwitchAxisMessageData()
        {
        }

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

        #region Methods

        public override string ToString()
        {
            return $"AxisToSwitch:{this.AxisToSwitch}";
        }

        #endregion
    }
}
