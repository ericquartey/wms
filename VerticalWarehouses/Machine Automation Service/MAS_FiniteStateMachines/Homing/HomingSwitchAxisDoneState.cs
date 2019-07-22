using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_FiniteStateMachines.Homing
{
    public class HomingSwitchAxisDoneState : StateBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly HomingStateMachine parentHomingStateMachine;

        private bool disposed;

        #endregion

        #region Constructors

        public HomingSwitchAxisDoneState(
            IStateMachine parentMachine,
            Axis axisToCalibrate,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.axisToCalibrate = axisToCalibrate;
            this.parentHomingStateMachine = parentMachine as HomingStateMachine;
        }

        #endregion

        #region Destructors

        ~HomingSwitchAxisDoneState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.CalibrateAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState(new HomingCalibrateAxisDoneState(this.ParentStateMachine, this.axisToCalibrate, this.Logger));
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new HomingErrorState(this.ParentStateMachine, this.axisToCalibrate, message, this.Logger));
                        break;
                }
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var calibrateAxisData = new CalibrateAxisFieldMessageData(this.axisToCalibrate);
            var commandMessage = new FieldCommandMessage(
                calibrateAxisData,
                $"Homing {this.axisToCalibrate} State Started",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.CalibrateAxis);

            this.Logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            string description = (this.parentHomingStateMachine != null && this.parentHomingStateMachine.GetMaxSteps() > 1 ?
                string.Format(" {0}/{1}", this.parentHomingStateMachine.GetNumberOfExecutedSteps() + 1, this.parentHomingStateMachine.GetMaxSteps()) :
                string.Empty);
            var notificationMessageData = new CalibrateAxisMessageData(this.axisToCalibrate, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"{this.axisToCalibrate} axis calibration started{description}",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.CalibrateAxis,
                MessageStatus.OperationStart);

            this.Logger.LogTrace($"2:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new HomingEndState(this.ParentStateMachine, this.axisToCalibrate, this.Logger, true));
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
