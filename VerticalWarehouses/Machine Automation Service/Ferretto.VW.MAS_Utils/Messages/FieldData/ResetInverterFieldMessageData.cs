using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class ResetInverterFieldMessageData : IResetInverterFieldMessageData
    {
        #region Constructors

        public ResetInverterFieldMessageData(Axis axisToCalibrate, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisToStop = axisToCalibrate;
            this.Verbosity = verbosity;
        }

        public ResetInverterFieldMessageData(ShutterPosition shutterPosition, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.ShutterPosition = shutterPosition;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public Axis AxisToStop { get; }

        public ShutterPosition ShutterPosition { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
