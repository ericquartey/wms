using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.RepetitiveHorizontalMovements.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.RepetitiveHorizontalMovements
{
    internal class RepetitiveHorizontalMovementsEndState : StateBase
    {
        #region Fields

        private readonly IRepetitiveHorizontalMovementsMachineData machineData;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly IServiceScope scope;

        private readonly IRepetitiveHorizontalMovementsStateData stateData;

        #endregion

        #region Constructors

        public RepetitiveHorizontalMovementsEndState(IRepetitiveHorizontalMovementsStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IRepetitiveHorizontalMovementsMachineData;
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
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source}");

            if (MessageType.Positioning == message.Type)
            {
                // Publish a notification message about the stop of operation
                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
                    "Repetitive Horizontal Movements Stopped",
                    MessageActor.DeviceManager,
                    MessageActor.DeviceManager,
                    MessageType.RepetitiveHorizontalMovements,
                    this.machineData.RequestingBay,
                    this.machineData.TargetBay,
                    StopRequestReasonConverter.GetMessageStatusFromReason(/*StopRequestReason.Stop*/this.stateData.StopRequestReason));

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                this.Logger.LogDebug("FSM Repetitive Horizontal Movements Stopped");

                // Restore the machine manual mode
                //this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                //this.Logger.LogInformation($"Machine status switched to {MachineMode.Manual}");

                this.machineVolatileDataProvider.Mode = this.machineVolatileDataProvider.GetMachineModeManualByBayNumber(this.machineData.TargetBay);
                this.Logger.LogInformation($"Machine status switched to {this.machineVolatileDataProvider.Mode}");
            }
        }

        public override void Start()
        {
            this.Logger.LogDebug($"1:Start {this.GetType().Name} StopRequestReason {this.stateData.StopRequestReason}");

            if (this.stateData.StopRequestReason != StopRequestReason.NoReason)
            {
                // Send a stop command message to Positioning state machine
                var messageData = new StopMessageData(StopRequestReason.Stop);
                var message = new CommandMessage(
                    messageData,
                    "Stop Elevator Command",
                    MessageActor.DeviceManager,
                    MessageActor.DeviceManager,
                    MessageType.Stop,
                    this.machineData.RequestingBay,
                    BayNumber.ElevatorBay);
                this.ParentStateMachine.PublishCommandMessage(message);
            }
            else
            {
                // Publish a notification message about the completion of operation
                var notificationMessage = new NotificationMessage(
                     this.machineData.MessageData,
                     "Repetitive Horizontal Movements Test Completed",
                     MessageActor.DeviceManager,
                     MessageActor.DeviceManager,
                     MessageType.RepetitiveHorizontalMovements,
                     this.machineData.RequestingBay,
                     this.machineData.TargetBay,
                     StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                this.Logger.LogDebug("FSM Repetitive Horizontal Movements End");

                // Restore the machine manual mode
                //this.machineVolatileDataProvider.Mode = MachineMode.Manual;
                //this.Logger.LogInformation($"Machine status switched to {MachineMode.Manual}");

                this.machineVolatileDataProvider.Mode = this.machineVolatileDataProvider.GetMachineModeManualByBayNumber(this.machineData.RequestingBay);
                this.Logger.LogInformation($"Machine status switched to {this.machineVolatileDataProvider.Mode}");
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("Retry Stop Command");
            this.Start();
        }

        #endregion
    }
}
