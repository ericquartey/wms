using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer
{
    internal class MoveDrawerCradleState : StateBase
    {

        #region Fields

        private readonly IMoveDrawerMachineData machineData;

        private readonly IMoveDrawerStateData stateData;

        private bool disposed;

        private PositioningMessageData positioningMessageData;

        #endregion

        #region Constructors

        public MoveDrawerCradleState(IMoveDrawerStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IMoveDrawerMachineData;
        }

        #endregion

        #region Destructors

        ~MoveDrawerCradleState()
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
            //TODO when Inverter Driver notifies completion of Positioning of the drawer move to next state
            if (message.Type == FieldMessageType.Positioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:

                        // TEMP Check sensors' status
                        // NOTE: Comment the line about the sensor check, if you use it with Bender
                        if (!this.machineData.MachineSensorsStatus.IsDrawerCompletelyOnCradle)
                        {
                            var notificationMessage = new NotificationMessage(
                                null,
                                "Cradle is not completely loaded",
                                MessageActor.Any,
                                MessageActor.FiniteStateMachines,
                                MessageType.DrawerOperation,
                                this.machineData.RequestingBay,
                                this.machineData.TargetBay,
                                MessageStatus.OperationError,
                                ErrorLevel.Error,
                                MessageVerbosity.Error);

                            using (var scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope())
                            {
                                var errorsProvider = scope.ServiceProvider.GetRequiredService<IErrorsProvider>();

                                errorsProvider.RecordNew(MachineErrors.CradleNotCompletelyLoaded, this.machineData.RequestingBay);
                            }

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

                            this.stateData.FieldMessage = message;
                            this.ParentStateMachine.ChangeState(new MoveDrawerErrorState(this.stateData));

                            return;
                        }

                        if (this.machineData.DrawerOperationData.Step == DrawerOperationStep.StoringDrawerToCell ||
                            this.machineData.DrawerOperationData.Step == DrawerOperationStep.StoringDrawerToBay)
                        {
                            this.ParentStateMachine.ChangeState(new MoveDrawerEndState(this.stateData));
                        }

                        if (this.machineData.DrawerOperationData.Step == DrawerOperationStep.LoadingDrawerFromBay ||
                            this.machineData.DrawerOperationData.Step == DrawerOperationStep.LoadingDrawerFromCell)
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
            //TODO Send horizontal Positioning to inverter driver, loading positioning data from data layer, based on current drawer position read from sensors
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
                this.machineData.DrawerOperationData.Step,
                MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"Moving cradle",
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
            decimal target = 0;

            ////TEMP: Remove the hardcoded value (used only for test)
            //if (this.drawerOperationData.Step == DrawerOperationStep.LoadingDrawerFromBay) //(this.drawerOperationStep == DrawerOperationStep.LoadingDrawerFromBay)
            //{
            //    target = +150;
            //}

            //if (this.drawerOperationData.Step == DrawerOperationStep.LoadingDrawerFromCell)   // (this.drawerOperationStep == DrawerOperationStep.LoadingDrawerFromCell)
            //{
            //    // TODO Get the coordinate of cell (use the dataLayer specialized interface??)
            //    // Use the side in order to get the correct sign of movement
            //    target = -150;
            //}

            //if (this.drawerOperationData.Step == DrawerOperationStep.StoringDrawerToBay)  // (this.drawerOperationStep == DrawerOperationStep.StoringDrawerToBay)
            //{
            //    target = -150;
            //}

            //if (this.drawerOperationData.Step == DrawerOperationStep.StoringDrawerToCell)   // (this.drawerOperationStep == DrawerOperationStep.StoringDrawerToCell)
            //{
            //    // TODO Get the coordinate of cell (use the dataLayer specialized interface??)
            //    // Use the side in order to get the correct sign of movement
            //    target = +150;
            //}

            if (this.drawerOperationData.Step == DrawerOperationStep.LoadingDrawerFromBay || this.drawerOperationData.Step == DrawerOperationStep.LoadingDrawerFromCell)
            {
                target = this.drawerOperationData.SourceHorizontalPosition;
            }
            else
            {
                target = this.drawerOperationData.DestinationHorizontalPosition;
            }

            //TEMP: The acceleration and speed parameters are provided by the vertimagConfiguration file (used only for test)
            var maxSpeed = this.horizontalAxis.MaxEmptySpeedHA;
            decimal[] maxAcceleration = { this.horizontalAxis.MaxEmptyAccelerationHA };
            decimal[] maxDeceleration = { this.horizontalAxis.MaxEmptyDecelerationHA };
            decimal[] switchPosition = { 0 };
            var feedRate = 0.10; // TEMP: remove this code line (used only for test)

            decimal[] speed = { maxSpeed * (decimal)feedRate };

            this.positioningMessageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Relative,
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
                (target >= 0 ? HorizontalMovementDirection.Forwards : HorizontalMovementDirection.Backwards));
        }

        #endregion
    }
}
