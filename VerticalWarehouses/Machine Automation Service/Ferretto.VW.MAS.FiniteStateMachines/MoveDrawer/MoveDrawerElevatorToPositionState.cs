using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer
{
    internal class MoveDrawerElevatorToPositionState : StateBase
    {
        #region Fields

        private readonly IMoveDrawerMachineData machineData;

        private readonly PositioningMessageData positioningMessageData;

        private readonly IMoveDrawerStateData stateData;

        private bool disposed;

        #endregion

        #region Constructors

        public MoveDrawerElevatorToPositionState(IMoveDrawerStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IMoveDrawerMachineData;
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

                        var currentStep = this.machineData.DrawerOperationData.Step;
                        if (currentStep == DrawerOperationStep.None)
                        {
                            this.ParentStateMachine.ChangeState(new MoveDrawerSwitchAxisState(this.stateData));
                        }

                        if (currentStep == DrawerOperationStep.MovingElevatorUp ||
                            currentStep == DrawerOperationStep.MovingElevatorDown)
                        {
                            this.ParentStateMachine.ChangeState(new MoveDrawerSwitchAxisState(this.stateData));
                        }

                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.ParentStateMachine.ChangeState(new MoveDrawerErrorState(this.stateData));
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

            this.Logger.LogDebug($"Started Positioning to {this.machineData.DrawerOperationData.Source}");

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
                this.machineData.DrawerOperationData.Operation,
                DrawerOperationStep.MovingElevatorUp,
                MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"Moving elevator",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.DrawerOperation,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Start");
            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new MoveDrawerEndState(this.stateData));
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

            var target = this.machineData.DrawerOperationData.Step == DrawerOperationStep.None
               ? this.machineData.DrawerOperationData.SourceVerticalPosition
               : this.machineData.DrawerOperationData.DestinationVerticalPosition;

            // TEMP: The acceleration and speed parameters are provided by the vertimagConfiguration file (used only for test)
            /*
             *
             *
             *

            var maxSpeed = this.machineData.VerticalAxis.MaxEmptySpeed;
            double[] maxAcceleration = { this.machineData.VerticalAxis.MaxEmptyAcceleration };
            double[] maxDeceleration = { this.machineData.VerticalAxis.MaxEmptyDeceleration };
            double[] switchPosition = { 0 };
            var feedRate = 0.10;  // TEMP: remove this code line (used only for test)

            double[] speed = { maxSpeed * (double)feedRate };

            var direction = target > this.machineData.DrawerOperationData.SourceHorizontalPosition
                ? HorizontalMovementDirection.Forwards
                : HorizontalMovementDirection.Backwards;

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
                switchPosition,
                direction);

             */
        }

        #endregion
    }
}
