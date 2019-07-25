using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer
{
    public class MoveDrawerSwitchAxisState : StateBase
    {
        #region Fields

        private readonly IDrawerOperationMessageData drawerOperationData;

        private readonly IGeneralInfoDataLayer generalInfoDataLayer;

        private readonly IHorizontalAxis horizontalAxis;

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private readonly Axis targetAxis;

        private readonly IVerticalAxis verticalAxis;

        private bool disposed;

        private bool inverterSwitched;

        private bool ioSwitched;

        #endregion

        #region Constructors

        public MoveDrawerSwitchAxisState(
            IStateMachine parentMachine,
            Axis targetAxis,
            IDrawerOperationMessageData drawerOperationData,
            IGeneralInfoDataLayer generalInfoDataLayer,
            IVerticalAxis verticalAxis,
            IHorizontalAxis horizontalAxis,
            IMachineSensorsStatus machineSensorsStatus,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.targetAxis = targetAxis;
            this.generalInfoDataLayer = generalInfoDataLayer;
            this.verticalAxis = verticalAxis;
            this.horizontalAxis = horizontalAxis;
            this.machineSensorsStatus = machineSensorsStatus;
            this.drawerOperationData = drawerOperationData;
        }

        #endregion

        #region Destructors

        ~MoveDrawerSwitchAxisState()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process CommandMessage {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogDebug($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            //TODO When IODriver and Inverter Driver report Axis Switch move to next state
            if (message.Type == FieldMessageType.SwitchAxis)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ioSwitched = true;
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new MoveDrawerErrorState(this.ParentStateMachine, message, this.drawerOperationData, this.targetAxis, this.Logger));
                        break;
                }
            }

            if (message.Type == FieldMessageType.InverterSwitchOn)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.inverterSwitched = true;
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new MoveDrawerErrorState(this.ParentStateMachine, message, this.drawerOperationData, this.targetAxis, this.Logger));
                        break;
                }
            }

            if (this.ioSwitched && this.inverterSwitched)
            {
                var currentStep = this.drawerOperationData.Step;

                if (currentStep == DrawerOperationStep.None)
                {
                    var nextStep = DrawerOperationStep.None;
                    switch (this.drawerOperationData.Source)
                    {
                        case DrawerDestination.CarouselBay1Down:
                        case DrawerDestination.CarouselBay1Up:
                        case DrawerDestination.CarouselBay2Down:
                        //...
                        case DrawerDestination.InternalBay1Up:
                        case DrawerDestination.InternalBay2Up:
                            nextStep = DrawerOperationStep.LoadingDrawerFromBay;
                            break;

                        case DrawerDestination.Cell:
                            nextStep = DrawerOperationStep.LoadingDrawerFromCell;
                            break;

                        default:
                            break;
                    }

                    this.drawerOperationData.Step = nextStep;

                    this.ParentStateMachine.ChangeState(new MoveDrawerCradleState(
                        this.ParentStateMachine,
                        this.drawerOperationData,
                        this.generalInfoDataLayer,
                        this.verticalAxis,
                        this.horizontalAxis,
                        this.machineSensorsStatus,
                        this.Logger));
                }

                if (currentStep == DrawerOperationStep.LoadingDrawerFromBay)
                {
                    this.drawerOperationData.Step = DrawerOperationStep.MovingElevatorUp;

                    this.ParentStateMachine.ChangeState(new MoveDrawerElevatorToPositionState(
                        this.ParentStateMachine,
                        this.drawerOperationData,
                        this.generalInfoDataLayer,
                        this.verticalAxis,
                        this.horizontalAxis,
                        this.machineSensorsStatus,
                        this.Logger));
                }

                if (currentStep == DrawerOperationStep.LoadingDrawerFromCell)
                {
                    this.drawerOperationData.Step = DrawerOperationStep.MovingElevatorDown;

                    this.ParentStateMachine.ChangeState(new MoveDrawerElevatorToPositionState(
                        this.ParentStateMachine,
                        this.drawerOperationData,
                        this.generalInfoDataLayer,
                        this.verticalAxis,
                        this.horizontalAxis,
                        this.machineSensorsStatus,
                        this.Logger));
                }

                if (currentStep == DrawerOperationStep.MovingElevatorUp)
                {
                    var nextStep = DrawerOperationStep.None;
                    switch (this.drawerOperationData.Destination)
                    {
                        case DrawerDestination.CarouselBay1Down:
                        case DrawerDestination.CarouselBay1Up:
                        case DrawerDestination.CarouselBay2Down:
                        //...
                        case DrawerDestination.InternalBay1Up:
                        case DrawerDestination.InternalBay2Up:
                            nextStep = DrawerOperationStep.StoringDrawerToBay;
                            break;

                        case DrawerDestination.Cell:
                            nextStep = DrawerOperationStep.StoringDrawerToCell;
                            break;

                        default:
                            break;
                    }

                    this.drawerOperationData.Step = nextStep;

                    this.ParentStateMachine.ChangeState(new MoveDrawerCradleState(
                        this.ParentStateMachine,
                        this.drawerOperationData,
                        this.generalInfoDataLayer,
                        this.verticalAxis,
                        this.horizontalAxis,
                        this.machineSensorsStatus,
                        this.Logger));
                }

                if (currentStep == DrawerOperationStep.MovingElevatorDown)
                {
                    var nextStep = DrawerOperationStep.None;
                    switch (this.drawerOperationData.Destination)
                    {
                        case DrawerDestination.CarouselBay1Down:
                        case DrawerDestination.CarouselBay1Up:
                        case DrawerDestination.CarouselBay2Down:
                        //...
                        case DrawerDestination.InternalBay1Up:
                        case DrawerDestination.InternalBay2Up:
                            nextStep = DrawerOperationStep.StoringDrawerToBay;
                            break;

                        case DrawerDestination.Cell:
                            nextStep = DrawerOperationStep.StoringDrawerToCell;
                            break;

                        default:
                            break;
                    }

                    this.drawerOperationData.Step = nextStep;

                    this.ParentStateMachine.ChangeState(new MoveDrawerCradleState(
                        this.ParentStateMachine,
                        this.drawerOperationData,
                        this.generalInfoDataLayer,
                        this.verticalAxis,
                        this.horizontalAxis,
                        this.machineSensorsStatus,
                        this.Logger));
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            //TODO Send Switch Axis commands to IODriver and Inverter Driver. Destination axis is provide by constructor

            var ioCommandMessageData = new SwitchAxisFieldMessageData(this.targetAxis);
            var ioCommandMessage = new FieldCommandMessage(
                ioCommandMessageData,
                $"Switch Axis {this.targetAxis}",
                FieldMessageActor.IoDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.SwitchAxis);

            this.Logger.LogDebug($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);

            //TODO Check if hard coding inverter index on MainInverter is correct or a dynamic selection of inverter index is required
            var inverterCommandMessageData = new InverterSwitchOnFieldMessageData(this.targetAxis, InverterIndex.MainInverter);
            var inverterCommandMessage = new FieldCommandMessage(
                inverterCommandMessageData,
                $"Switch Axis {Axis.Vertical}",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.InverterSwitchOn);

            this.Logger.LogDebug($"2:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

            // Send a notification message about the start operation for MessageType.DrawerOperation
            var notificationMessageData = new DrawerOperationMessageData(
                this.drawerOperationData.Operation,
                DrawerOperationStep.LoadingDrawerFromBay,
                MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"Switch axis",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.DrawerOperation,
                MessageStatus.OperationStart);

            this.Logger.LogDebug($"3:Publishing Automation Notification Message {notificationMessage.Type} Destination {notificationMessage.Destination} Status {notificationMessage.Status}");

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop()
        {
            this.ParentStateMachine.ChangeState(new MoveDrawerEndState(
                this.ParentStateMachine,
                this.drawerOperationData,
                this.Logger,
                true));
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
