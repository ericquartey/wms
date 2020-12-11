using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.RepetitiveHorizontalMovements.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.RepetitiveHorizontalMovements
{
    internal class RepetitiveHorizontalMovementsErrorState : StateBase
    {
        #region Fields

        private readonly IRepetitiveHorizontalMovementsMachineData machineData;

        private readonly IMachineVolatileDataProvider machineVolatileDataProvider;

        private readonly IServiceScope scope;

        private readonly IRepetitiveHorizontalMovementsStateData stateData;

        #endregion

        #region Constructors

        public RepetitiveHorizontalMovementsErrorState(
            IRepetitiveHorizontalMovementsStateData stateData,
            ILogger logger)
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
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == MessageType.Positioning && message.Status == MessageStatus.OperationError)
            {
                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
                    $"{this.machineData.MessageData} Repetitive horizontal movements Error Detected",
                    MessageActor.DeviceManager,
                    MessageActor.DeviceManager,
                    MessageType.RepetitiveHorizontalMovements,
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
                $"{this.machineData.MessageData} Repetitive horizontal movements Error Detected",
                MessageActor.DeviceManager,
                MessageActor.DeviceManager,
                MessageType.RepetitiveHorizontalMovements,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationError);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);

            // Change the machine mode
            //this.machineVolatileDataProvider.Mode = MachineMode.Manual;
            //this.Logger.LogInformation($"Machine status switched to {MachineMode.Manual}");

            this.machineVolatileDataProvider.Mode = this.machineVolatileDataProvider.GetMachineModeManualByBayNumber(this.machineData.TargetBay);
            this.Logger.LogInformation($"Machine status switched to {this.machineVolatileDataProvider.Mode}");
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Empty");
        }

        #endregion
    }
}
