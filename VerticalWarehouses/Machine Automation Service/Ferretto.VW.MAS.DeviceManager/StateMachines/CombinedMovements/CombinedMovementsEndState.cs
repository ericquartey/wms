using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DeviceManager.CombinedMovements.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.CombinedMovements
{
    internal class CombinedMovementsEndState : StateBase
    {
        #region Fields

        private readonly ICombinedMovementsMachineData machineData;

        private readonly IServiceScope scope;

        private readonly ICombinedMovementsStateData stateData;

        #endregion

        #region Constructors

        public CombinedMovementsEndState(ICombinedMovementsStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as ICombinedMovementsMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
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
            this.Logger.LogDebug($"1:Process Notification Message {message.Type} Source {message.Source}");  // LogTrace

            if (MessageType.Positioning == message.Type)
            {
                var data = message.Data as IPositioningMessageData;
                if (data.AxisMovement == Axis.Vertical)
                {
                    this.machineData.OnVerticalPositioningStopped = true;
                }
                if (data.AxisMovement == Axis.Horizontal)
                {
                    this.machineData.OnHorizontalPositioningStopped = true;
                }
            }

            var publishNotification = false;
            if (this.machineData.OnHorizontalPositioningError)
            {
                publishNotification = this.machineData.OnVerticalPositioningStopped;
            }

            if (this.machineData.OnVerticalPositioningError)
            {
                publishNotification = this.machineData.OnHorizontalPositioningStopped;
            }

            if (!publishNotification)
            {
                publishNotification = this.machineData.OnHorizontalPositioningStopped &&
                    this.machineData.OnVerticalPositioningStopped;
            }

            if (publishNotification)
            {
                // Publish a notification message about the stop of operation
                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
                    "Combined Movements Stopped",
                    MessageActor.DeviceManager,
                    MessageActor.DeviceManager,
                    MessageType.CombinedMovements,
                    this.machineData.RequestingBay,
                    this.machineData.TargetBay,
                    StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                this.Logger.LogDebug("FSM Combined Movements Stopped");
            }
        }

        public override void Start()
        {
            this.Logger.LogDebug($"1:Start {this.GetType().Name} StopRequestReason {this.stateData.StopRequestReason}");

            switch (this.stateData.StopRequestReason)
            {
                case StopRequestReason.NoReason:
                    {
                        // Publish a notification message about the completion of operation
                        var notificationMessage = new NotificationMessage(
                             this.machineData.MessageData,
                             "Combined Movements Test Completed",
                             MessageActor.DeviceManager,
                             MessageActor.DeviceManager,
                             MessageType.CombinedMovements,
                             this.machineData.RequestingBay,
                             this.machineData.TargetBay,
                             StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

                        this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                        this.Logger.LogDebug("FSM Combined Movements End");

                        break;
                    }

                case StopRequestReason.RunningStateChanged:
                case StopRequestReason.Stop:
                    {
                        this.Logger.LogDebug($"No one Stop Command is sent to the {MessageActor.DeviceManager}");

                        break;
                    }

                // case StopRequestReason.Stop:
                case StopRequestReason.Abort:
                case StopRequestReason.Error:
                case StopRequestReason.FaultStateChanged:
                    {
                        this.Logger.LogDebug($"Send Stop Elevator Command from {this.ParentStateMachine.ToString()} to {MessageActor.DeviceManager}");

                        // Send a stop command message to Positioning state machine(s)
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

                        break;
                    }
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            // do not repeat this stop command or there will be an infinite loop

            //this.Logger.LogDebug($"1: Retry Stop Command. Reason:{reason}");
            //this.Start();

            // quit fsm
            var notificationMessage = new NotificationMessage(
                this.machineData.MessageData,
                "Combined Movements Stopped",
                MessageActor.DeviceManager,
                MessageActor.DeviceManager,
                MessageType.CombinedMovements,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                StopRequestReasonConverter.GetMessageStatusFromReason(this.stateData.StopRequestReason));

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            this.Logger.LogDebug("FSM Combined Movements Stopped");
        }

        #endregion
    }
}
