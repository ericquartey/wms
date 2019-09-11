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
    public class MoveDrawerElevatorToPositionState : StateBase
    {
        #region Fields

        private readonly IDrawerOperationMessageData drawerOperationData;

        private readonly IGeneralInfoConfigurationDataLayer generalInfoDataLayer;

        private readonly IHorizontalAxisDataLayer horizontalAxis;

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private readonly IVerticalAxisDataLayer verticalAxis;

        private bool disposed;

        private PositioningMessageData positioningMessageData;

        #endregion

        #region Constructors

        public MoveDrawerElevatorToPositionState(
            IStateMachine parentMachine,
            IDrawerOperationMessageData drawerOperationData,
            IGeneralInfoConfigurationDataLayer generalInfoDataLayer,
            IVerticalAxisDataLayer verticalAxis,
            IHorizontalAxisDataLayer horizontalAxis,
            IMachineSensorsStatus machineSensorsStatus,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.drawerOperationData = drawerOperationData;
            this.generalInfoDataLayer = generalInfoDataLayer;
            this.verticalAxis = verticalAxis;
            this.horizontalAxis = horizontalAxis;
            this.machineSensorsStatus = machineSensorsStatus;
        }

        #endregion

        #region Destructors

        ~MoveDrawerElevatorToPositionState()
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
            //TODO when Inverter Driver notifies completion of Positioning at the destination level move to next state
            if (message.Type == FieldMessageType.Positioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:

                        var currentStep = this.drawerOperationData.Step;
                        if (currentStep == DrawerOperationStep.None)
                        {
                            this.drawerOperationData.Step = DrawerOperationStep.None;

                            this.ParentStateMachine.ChangeState(new MoveDrawerSwitchAxisState(
                                this.ParentStateMachine,
                                Axis.Horizontal,
                                this.drawerOperationData,
                                this.generalInfoDataLayer,
                                this.verticalAxis,
                                this.horizontalAxis,
                                this.machineSensorsStatus,
                                this.Logger));
                        }

                        if (currentStep == DrawerOperationStep.MovingElevatorUp ||
                            currentStep == DrawerOperationStep.MovingElevatorDown)
                        {
                            this.ParentStateMachine.ChangeState(new MoveDrawerSwitchAxisState(
                                this.ParentStateMachine,
                                Axis.Horizontal,
                                this.drawerOperationData,
                                this.generalInfoDataLayer,
                                this.verticalAxis,
                                this.horizontalAxis,
                                this.machineSensorsStatus,
                                this.Logger));
                        }

                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new MoveDrawerErrorState(this.ParentStateMachine, message, this.drawerOperationData, Axis.Vertical, this.Logger));
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process NotificationMessage {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            //TODO Send Vertical Positioning command to inverter driver, loading positioning data from data layer
            this.GetParameters();

            this.Logger.LogDebug($"Started Positioning to {this.drawerOperationData.Source}");

            var positioningFieldMessageData = new PositioningFieldMessageData(this.positioningMessageData);

            var commandMessage = new FieldCommandMessage(
                positioningFieldMessageData,
                $"{this.positioningMessageData.AxisMovement} Positioning State Started",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.Positioning,
                (byte)InverterIndex.MainInverter);

            this.Logger.LogDebug($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            // Send a notification message about the start operation for move elevator of MessageType.DrawerOperation
            var notificationMessageData = new DrawerOperationMessageData(
                this.drawerOperationData.Operation,
                DrawerOperationStep.MovingElevatorUp,
                MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"Moving elevator",
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

        //TEMP Check this code
        private void GetParameters()
        {
            decimal target = 0;

            //if (this.drawerOperationData.Step == DrawerOperationStep.None) //(this.drawerOperationStep == DrawerOperationStep.None)
            //{
            //    if (this.drawerOperationData.Source == DrawerDestination.Cell)
            //    {
            //        // TODO Get the coordinate of cell (use the dataLayer specialized interface??)

            //        target = 100;
            //    }
            //    else
            //    {
            //        switch (this.drawerOperationData.Source)
            //        {
            //            case DrawerDestination.CarouselBay1Up:
            //            case DrawerDestination.ExternalBay1Up:
            //            case DrawerDestination.InternalBay1Up:
            //                target = this.generalInfoDataLayer.Bay1Position1;
            //                break;

            //            case DrawerDestination.CarouselBay1Down:
            //            case DrawerDestination.ExternalBay1Down:
            //            case DrawerDestination.InternalBay1Down:
            //                target = this.generalInfoDataLayer.Bay1Position2;
            //                break;

            //            case DrawerDestination.CarouselBay2Up:
            //            case DrawerDestination.ExternalBay2Up:
            //            case DrawerDestination.InternalBay2Up:
            //                target = this.generalInfoDataLayer.Bay2Position1;
            //                break;

            //            // Add other destinations here

            //            default:
            //                break;
            //        }

            //        target /= 10; // TEMP: remove this code line (used only for test)
            //    }
            //}
            //else
            //{
            //    if (this.drawerOperationData.Destination == DrawerDestination.Cell)
            //    {
            //        // TODO Get the coordinate of cell (use the dataLayer specialized interface??)
            //        target = 100;
            //    }
            //    else
            //    {
            //        switch (this.drawerOperationData.Destination)
            //        {
            //            case DrawerDestination.CarouselBay1Up:
            //            case DrawerDestination.ExternalBay1Up:
            //            case DrawerDestination.InternalBay1Up:
            //                target = this.generalInfoDataLayer.Bay1Position1;
            //                break;

            //            case DrawerDestination.CarouselBay1Down:
            //            case DrawerDestination.ExternalBay1Down:
            //            case DrawerDestination.InternalBay1Down:
            //                target = this.generalInfoDataLayer.Bay1Position2;
            //                // TODO
            //                break;

            //            case DrawerDestination.CarouselBay2Up:
            //            case DrawerDestination.ExternalBay2Up:
            //            case DrawerDestination.InternalBay2Up:
            //                target = this.generalInfoDataLayer.Bay2Position1;
            //                break;

            //            // Add other destinations here

            //            default:
            //                break;
            //        }

            //        target /= 10;  // TEMP: remove this code line (used only for test)
            //    }
            //}

            if (this.drawerOperationData.Step == DrawerOperationStep.None)
                {
                target = this.drawerOperationData.SourceVerticalPosition;
                }
                else
                {
                target = this.drawerOperationData.DestinationVerticalPosition;
            }

            //TEMP: The acceleration and speed parameters are provided by the vertimagConfiguration file (used only for test)
            var maxSpeed = this.verticalAxis.MaxEmptySpeed;
            decimal[] maxAcceleration = { this.verticalAxis.MaxEmptyAcceleration };
            decimal[] maxDeceleration = { this.verticalAxis.MaxEmptyDeceleration };
            decimal[] switchPosition = { 0 };
            var feedRate = 0.10;  // TEMP: remove this code line (used only for test)

            decimal[] speed = { maxSpeed * (decimal)feedRate };

            this.positioningMessageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                MovementMode.Position,
                target,
                speed,
                maxAcceleration,
                maxDeceleration,
                0,
                0,
                0,
                0,
                switchPosition);
        }

        #endregion
    }
}
