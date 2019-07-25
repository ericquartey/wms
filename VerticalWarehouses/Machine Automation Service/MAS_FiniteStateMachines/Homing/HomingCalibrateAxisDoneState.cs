using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.Homing
{
    public class HomingCalibrateAxisDoneState : StateBase
    {
        #region Fields

        private readonly Axis axisToSwitch;

        private readonly Axis axisToSwitched;

        private readonly int currentStep;

        private readonly int maxStep;

        private bool disposed;

        private bool inverterSwitched;

        private bool ioSwitched;

        #endregion

        #region Constructors

        public HomingCalibrateAxisDoneState(
            IStateMachine parentMachine,
            Axis axisCalibrated,
            int currentStepCalibrated,
            int maxStepCalibrate,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.axisToSwitched = axisCalibrated;
            this.axisToSwitch = axisCalibrated == Axis.Horizontal
                ? Axis.Vertical
                : Axis.Horizontal;
            this.currentStep = (parentMachine as HomingStateMachine)?.GetNumberOfExecutedSteps() ?? 0;
            this.maxStep = (parentMachine as HomingStateMachine)?.GetMaxSteps() ?? 0;
        }

        #endregion

        #region Destructors

        ~HomingCalibrateAxisDoneState()
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

            if (message.Type == FieldMessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ioSwitched = true;

                        // add
                        //var inverterCommandMessageData = new InverterSwitchOnFieldMessageData(this.axisToSwitch, InverterIndex.MainInverter);
                        //var inverterCommandMessage = new FieldCommandMessage(
                        //    inverterCommandMessageData,
                        //    $"Switch Axis {this.axisToSwitch}",
                        //    FieldMessageActor.InverterDriver,
                        //    FieldMessageActor.FiniteStateMachines,
                        //    FieldMessageType.InverterSwitchOn);

                        //this.Logger.LogTrace($"2:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

                        //this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

                        //

                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new HomingErrorState(this.ParentStateMachine, this.axisToSwitch, message, this.Logger));
                        break;
                }
            }

            if (message.Type == FieldMessageType.InverterSwitchOn)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationExecuting:
                        var notificationMessageData = new CalibrateAxisMessageData(this.axisToSwitch, this.currentStep, this.maxStep, MessageVerbosity.Info);
                        var notificationMessage = new NotificationMessage(
                            notificationMessageData,
                            $"{this.axisToSwitch} axis calibration executing",
                            MessageActor.Any,
                            MessageActor.FiniteStateMachines,
                            MessageType.CalibrateAxis,
                            MessageStatus.OperationExecuting);

                        this.Logger.LogTrace($"2:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

                        this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                        break;

                    case MessageStatus.OperationEnd:
                        this.inverterSwitched = true;
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new HomingErrorState(this.ParentStateMachine, this.axisToSwitch, message, this.Logger));
                        break;
                }
            }

            if (this.ioSwitched && this.inverterSwitched)
            {
                this.ParentStateMachine.ChangeState(new HomingSwitchAxisDoneState(this.ParentStateMachine, this.axisToSwitch, this.Logger));
            }
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        /// <inheritdoc/>
        public override void Start()
        {
            var ioCommandMessageData = new SwitchAxisFieldMessageData(this.axisToSwitch);
            var ioCommandMessage = new FieldCommandMessage(
                ioCommandMessageData,
                $"Switch Axis {this.axisToSwitch}",
                FieldMessageActor.IoDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.SwitchAxis);

            this.Logger.LogTrace($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);

            //TODO Check if hard coding inverter index on MainInverter is correct or a dynamic selection of inverter index is required
            var inverterCommandMessageData = new InverterSwitchOnFieldMessageData(this.axisToSwitch, InverterIndex.MainInverter);
            var inverterCommandMessage = new FieldCommandMessage(
                inverterCommandMessageData,
                $"Switch Axis {this.axisToSwitch}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSwitchOn);

            this.Logger.LogTrace($"2:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

            var notificationMessageData = new CalibrateAxisMessageData(this.axisToSwitched, this.currentStep, this.maxStep, MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"{this.axisToSwitched} axis calibration completed",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.CalibrateAxis,
                MessageStatus.OperationEnd);

            this.Logger.LogTrace($"3:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop()
        {
            this.Logger.LogTrace("1:Method Start");

            this.ParentStateMachine.ChangeState(new HomingEndState(this.ParentStateMachine, this.axisToSwitch, this.Logger, true));
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
