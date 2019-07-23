using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.FiniteStateMachines.Interfaces;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_FiniteStateMachines.Interface;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages;
using Ferretto.VW.MAS_Utils.Messages.FieldData;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_FiniteStateMachines.MoveDrawer
{
    public class MoveDrawerElevatorToPositionState : StateBase
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IDrawerOperationMessageData drawerOperationData;

        private readonly IMachineSensorsStatus machineSensorsStatus;

        private bool disposed;

        private PositioningMessageData positioningMessageData;

        #endregion

        #region Constructors

        public MoveDrawerElevatorToPositionState(
            IStateMachine parentMachine,
            IDrawerOperationMessageData drawerOperationData,
            IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement,
            IMachineSensorsStatus machineSensorsStatus,
            ILogger logger)
            : base(parentMachine, logger)
        {
            this.drawerOperationData = drawerOperationData;
            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;
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
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            //TODO when Inverter Driver notifies completion of Positioning at the destination level move to next state
            if (message.Type == FieldMessageType.Positioning)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        this.ParentStateMachine.ChangeState(new MoveDrawerSwitchAxisState(this.ParentStateMachine, Axis.Horizontal, this.drawerOperationData, this.dataLayerConfigurationValueManagement, this.machineSensorsStatus, this.Logger));
                        break;

                    case MessageStatus.OperationError:
                        this.ParentStateMachine.ChangeState(new MoveDrawerErrorState(this.ParentStateMachine, message, this.Logger));
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
        }

        public override void Start()
        {
            //TODO Send Vertical Positioning command to inverter driver, loading positioning data from data layer

            this.getParameters();

            this.Logger.LogDebug($"Started Positioning to {this.drawerOperationData.Source}");

            var positioningFieldMessageData = new PositioningFieldMessageData(this.positioningMessageData);

            var commandMessage = new FieldCommandMessage(
                positioningFieldMessageData,
                $"{this.positioningMessageData.AxisMovement} Positioning State Started",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.Positioning);

            this.Logger.LogTrace($"1:Publishing Field Command Message {commandMessage.Type} Destination {commandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            // Send a notification message about the start operation for move elevator of MessageType.DrawerOperation
            var notificationMessageData = new DrawerOperationMessageData(
                this.drawerOperationData.Operation,
                DrawerOperationStep.MovingElevatorUp,
                MessageVerbosity.Info);
            var notificationMessage = new NotificationMessage(
                notificationMessageData,
                "Message Description",
                MessageActor.Any,
                MessageActor.FiniteStateMachines,
                MessageType.DrawerOperation,
                MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop()
        {
            this.ParentStateMachine.ChangeState(new MoveDrawerEndState(this.ParentStateMachine, this.drawerOperationData, this.dataLayerConfigurationValueManagement, this.machineSensorsStatus, this.Logger, true));
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

        private async Task getParameters()
        {
            decimal target = 0;

            if (this.drawerOperationData.Source != DrawerDestination.Cell)
            {
                var configValue = GeneralInfo.Undefined;
                switch (this.drawerOperationData.Source)
                {
                    case DrawerDestination.CarouselBay1Up:
                    case DrawerDestination.ExternalBay1Up:
                    case DrawerDestination.InternalBay1Up:
                        configValue = GeneralInfo.Bay1Position1;
                        break;

                    case DrawerDestination.CarouselBay1Down:
                    case DrawerDestination.ExternalBay1Down:
                    case DrawerDestination.InternalBay1Down:
                        configValue = GeneralInfo.Bay1Position2;
                        // TODO
                        break;

                    case DrawerDestination.CarouselBay2Up:
                    case DrawerDestination.ExternalBay2Up:
                    case DrawerDestination.InternalBay2Up:
                        configValue = GeneralInfo.Bay2Position1;
                        break;

                    // ...

                    default:
                        break;
                }

                target = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                         (long)configValue, (long)ConfigurationCategory.GeneralInfo);
            }
            else
            {
                // TODO Get the coordinate of cell (use the dataLayer specialized interface??)
            }

            var maxSpeed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxEmptySpeed, (long)ConfigurationCategory.VerticalAxis);
            var maxAcceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxEmptyAcceleration, (long)ConfigurationCategory.VerticalAxis);
            var maxDeceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxEmptyDeceleration, (long)ConfigurationCategory.VerticalAxis);
            var feedRate = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalManualMovements.FeedRate, (long)ConfigurationCategory.VerticalManualMovements);

            var speed = maxSpeed * feedRate;

            this.positioningMessageData = new PositioningMessageData(
                Axis.Vertical,
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
