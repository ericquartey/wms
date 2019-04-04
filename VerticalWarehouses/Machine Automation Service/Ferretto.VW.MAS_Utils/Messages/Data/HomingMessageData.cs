using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_Utils.Messages.Data
{
    public class HomingMessageData : IHomingMessageData
    {
        #region Constructors

        public HomingMessageData(Axis axisToCalibrate, MessageVerbosity verbosity = MessageVerbosity.Debug, FieldNotificationMessage fieldNotificationMessage = null)
        {
            this.AxisToCalibrate = axisToCalibrate;
            this.FieldMessage = fieldNotificationMessage;
            this.Verbosity = verbosity;
        }

        public HomingMessageData(ISwitchAxisFieldMessageData fieldMessageDate)
        {
            this.AxisToCalibrate = fieldMessageDate.AxisToSwitchOn;
            this.Verbosity = fieldMessageDate.Verbosity;
        }

        #endregion

        #region Properties

        public Axis AxisToCalibrate { get; }

        public FieldNotificationMessage FieldMessage { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
