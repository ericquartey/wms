using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.CommonUtils.Messages.Data
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
