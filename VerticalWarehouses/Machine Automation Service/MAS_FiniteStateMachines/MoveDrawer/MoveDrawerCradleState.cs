using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer
{
    public class MoveDrawerCradleState : StateBase
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IDrawerOperationMessageData drawerOperationData;

        private readonly DrawerOperationStep drawerOperationStep;

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private bool disposed;

        private PositioningMessageData positioningMessageData;

        #endregion

        #region Constructors

        public MoveDrawerCradleState(
            IStateMachine parentMachine,
            IDrawerOperationMessageData drawerOperationData,
            IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement,
            IMachineSensorsStatus machineSensorsStatus,
            DrawerOperationStep drawerOperationStep,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.drawerOperationData = drawerOperationData;
            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;
            this.machineSensorsStatus = machineSensorsStatus;
            this.drawerOperationStep = drawerOperationStep;
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

                        //TEMP Check sensors' status
                        if (!this.machineSensorsStatus.IsDrawerCompletelyOnCradle)
                        {
                            var notificationMessage = new NotificationMessage(
                                null,
                                "Cradle is not completely loaded",
                                MessageActor.Any,
                                MessageActor.FiniteStateMachines,
                                MessageType.DrawerOperation,
                                MessageStatus.OperationError,
                                ErrorLevel.Error,
                                MessageVerbosity.Error);

                            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

                            this.ParentStateMachine.ChangeState(new MoveDrawerErrorState(this.ParentStateMachine, message, this.drawerOperationData, Axis.Horizontal, this.Logger));

                            return;
                        }

                        if (this.drawerOperationStep == DrawerOperationStep.StoringDrawerToCell ||
                            this.drawerOperationStep == DrawerOperationStep.StoringDrawerToBay)
                        {
                            this.ParentStateMachine.ChangeState(new MoveDrawerEndState(
                                    this.ParentStateMachine,
                                    this.drawerOperationData,
                                    this.dataLayerConfigurationValueManagement,
                                    this.machineSensorsStatus,
                                    this.drawerOperationStep,
                                    this.Logger));
                        }

                        if (this.drawerOperationStep == DrawerOperationStep.LoadingDrawerFromBay ||
                            this.drawerOperationStep == DrawerOperationStep.LoadingDrawerFromCell)
                        {
                            this.ParentStateMachine.ChangeState(new MoveDrawerSwitchAxisState(
                                    this.ParentStateMachine,
                                    Axis.Vertical,
                                    this.drawerOperationData,
                                    this.dataLayerConfigurationValueManagement,
                                    this.machineSensorsStatus,
                                    this.drawerOperationStep,
                                    this.Logger));
                        }

                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new MoveDrawerErrorState(this.ParentStateMachine, message, this.drawerOperationData, Axis.Horizontal, this.Logger));
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
            this.getParameters();

            this.Logger.LogDebug($"Started Positioning to {this.drawerOperationData.Source}");

            var positioningFieldMessageData = new PositioningFieldMessageData(this.positioningMessageData);

            var commandMessage = new FieldCommandMessage(
                positioningFieldMessageData,
                $"{this.positioningMessageData.AxisMovement} Positioning State Started",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.Positioning);

            this.Logger.LogDebug($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            // Send a notification message about the start operation for move elevator of MessageType.DrawerOperation
            var notificationMessageData = new DrawerOperationMessageData(
                this.drawerOperationData.Operation,
                this.drawerOperationStep,
                MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                $"Moving cradle",
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
                this.dataLayerConfigurationValueManagement,
                this.machineSensorsStatus,
                this.drawerOperationStep,
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
        private async Task getParameters()
        {
            decimal target = 0;

            if (this.drawerOperationStep == DrawerOperationStep.LoadingDrawerFromBay)
            {
                target = 75;
            }

            if (this.drawerOperationStep == DrawerOperationStep.LoadingDrawerFromCell)
            {
                // TODO Get the coordinate of cell (use the dataLayer specialized interface??)
                target = 75;
            }

            if (this.drawerOperationStep == DrawerOperationStep.StoringDrawerToBay)
            {
                target = 110;
            }

            if (this.drawerOperationStep == DrawerOperationStep.StoringDrawerToCell)
            {
                // Take account the sign of movement to store the drawer

                // TODO Get the coordinate of cell (use the dataLayer specialized interface??)
                target = 110;
            }

            var maxSpeed = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)HorizontalAxis.MaxEmptySpeed, (long)ConfigurationCategory.HorizontalAxis);
            var maxAcceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)HorizontalAxis.MaxEmptyAcceleration, (long)ConfigurationCategory.HorizontalAxis);
            var maxDeceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)HorizontalAxis.MaxEmptyDeceleration, (long)ConfigurationCategory.HorizontalAxis);
            var feedRate = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)HorizontalManualMovements.FeedRate, (long)ConfigurationCategory.HorizontalManualMovements);

            var speed = maxSpeed * feedRate;

            this.positioningMessageData = new PositioningMessageData(
                Axis.Horizontal,
                MovementType.Absolute,
                target,
                speed,
                maxAcceleration,
                maxDeceleration,
                0,
                0,
                0);
        }

        #endregion
    }
}
