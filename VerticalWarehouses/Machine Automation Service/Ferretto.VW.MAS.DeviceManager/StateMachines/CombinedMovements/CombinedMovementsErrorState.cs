using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.CombinedMovements.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.CombinedMovements
{
    internal class CombinedMovementsErrorState : StateBase
    {
        #region Fields

        private readonly ICombinedMovementsMachineData machineData;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly IServiceScope scope;

        private readonly ICombinedMovementsStateData stateData;

        #endregion

        #region Constructors

        public CombinedMovementsErrorState(ICombinedMovementsStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as ICombinedMovementsMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
            this.machineVolatileDataProvider = this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source}");
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == MessageType.Positioning && message.Status == MessageStatus.OperationError)
            {
                var data = message.Data as IPositioningMessageData;

                var szMessage = string.Empty;
                switch (data.AxisMovement)
                {
                    case Axis.Horizontal:
                        {
                            this.machineData.OnHorizontalPositioningError = true;
                            szMessage = $"Stop Elevator Command for {Axis.Vertical} movement";
                            break;
                        }

                    case Axis.Vertical:
                        {
                            this.machineData.OnVerticalPositioningError = true;
                            szMessage = $"Stop Elevator Command for {Axis.Horizontal} movement";
                            break;
                        }

                    case Axis.HorizontalAndVertical:
                    case Axis.BayChain:
                    case Axis.None:
                        {
                            szMessage = $"Stop Elevator Command";
                            break;
                        }
                }

                this.Logger.LogDebug(szMessage);

                // Send a stop command message to Positioning state machine
                var stopMessageData = new StopMessageData(StopRequestReason.Stop);
                var stopMessage = new CommandMessage(
                    stopMessageData,
                    "Stop Elevator Command",
                    MessageActor.DeviceManager,
                    MessageActor.DeviceManager,
                    MessageType.Stop,
                    this.machineData.RequestingBay,
                    BayNumber.ElevatorBay);
                this.ParentStateMachine.PublishCommandMessage(stopMessage);

                // Send a notification message
                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
                    $"{this.machineData.MessageData} Combined movements Error Detected",
                    MessageActor.DeviceManager,
                    MessageActor.DeviceManager,
                    MessageType.CombinedMovements,
                    this.machineData.RequestingBay,
                    this.machineData.TargetBay,
                    MessageStatus.OperationError,
                    ErrorLevel.Error);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }
        }

        public override void Start()
        {
            this.Logger.LogDebug($"1:Start {this.GetType().Name} RequestingBay: {this.machineData.RequestingBay} TargetBay: {this.machineData.TargetBay}");

            var notificationMessage = new NotificationMessage(
                this.machineData.MessageData,
                $"{this.machineData.MessageData} Combined movements Error Detected",
                MessageActor.DeviceManager,
                MessageActor.DeviceManager,
                MessageType.CombinedMovements,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationError);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Empty");
        }

        #endregion
    }
}
