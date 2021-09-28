using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Homing;
using Ferretto.VW.MAS.DeviceManager.Homing.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.StateMachines.Homing
{
    internal class HomingBackExecutingState : StateBase, IDisposable
    {
        #region Fields

        private const int DefaultStatusWordPollingInterval = 100;

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly IHomingMachineData machineData;

        private readonly IServiceScope scope;

        private readonly IHomingStateData stateData;

        private bool isDisposed;

        private PositioningMessageData positioningData;

        #endregion

        #region Constructors

        public HomingBackExecutingState(IHomingStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IHomingMachineData;

            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();

            this.errorsProvider = this.scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.baysDataProvider = this.scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            this.Dispose(true);
        }

        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            switch (message.Status)
            {
                case MessageStatus.OperationExecuting:
                    switch (message.Type)
                    {
                        case FieldMessageType.InverterStatusUpdate when message.Data is InverterStatusUpdateFieldMessageData:
                            this.OnInverterStatusUpdated(message);
                            break;
                    }
                    break;

                case MessageStatus.OperationEnd:
                    switch (message.Type)
                    {
                        case FieldMessageType.Positioning:
                            this.Logger.LogDebug($"Trace Notification Message {message}");
                            this.ProcessEndPositioning();
                            break;

                        case FieldMessageType.InverterStop:
                            this.ProcessEndStop();
                            break;
                    }
                    break;

                case MessageStatus.OperationError:
                    this.stateData.FieldMessage = message;
                    this.ParentStateMachine.ChangeState(new HomingErrorState(this.stateData, this.Logger));
                    break;
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name} Inverter {this.machineData.CurrentInverterIndex} ");
            var inverterIndex = (byte)this.machineData.CurrentInverterIndex;

            var inverterDataMessage = new InverterSetTimerFieldMessageData(InverterTimer.SensorStatus, true, 250);
            var inverterMessage = new FieldCommandMessage(
                inverterDataMessage,
                "Update Inverter digital input status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                inverterIndex);

            this.Logger.LogTrace($"1:Publishing Field Command Message {inverterMessage.Type} Destination {inverterMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterMessage);

            var bay = this.baysDataProvider.GetByNumber(this.machineData.TargetBay);
            var targetPosition = bay.External.Race - this.baysDataProvider.GetChainPosition(this.machineData.TargetBay) + bay.ChainOffset;
            var speed = new[] { bay.FullLoadMovement.Speed };
            var acceleration = new[] { bay.FullLoadMovement.Acceleration };
            var deceleration = new[] { bay.FullLoadMovement.Deceleration };
            var switchPosition = new[] { 0.0 };
            this.positioningData = new PositioningMessageData(
                Axis.BayChain,
                MovementType.Relative,
                MovementMode.ExtBayChain,
                targetPosition,
                speed,
                acceleration,
                deceleration,
                switchPosition,
                HorizontalMovementDirection.Backwards);

            var positioningFieldMessageData = new PositioningFieldMessageData(this.positioningData, this.machineData.RequestingBay);

            var commandMessage = new FieldCommandMessage(
                positioningFieldMessageData,
                $"External {this.positioningData.AxisMovement} Positioning State Started",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.Positioning,
                inverterIndex);

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            this.ParentStateMachine.PublishFieldCommandMessage(
                new FieldCommandMessage(
                    new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, true, 100),
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                inverterIndex));

            var notificationMessage = new NotificationMessage(
                this.positioningData,
                $"External Bay Positioning Started",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.Positioning,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"1:Stop Method Start. Reason {reason}");

            this.stateData.StopRequestReason = reason;

            if (reason == StopRequestReason.Error)
            {
                this.ParentStateMachine.ChangeState(new HomingErrorState(this.stateData, this.Logger));
            }
            else
            {
                this.ParentStateMachine.ChangeState(new HomingEndState(this.stateData, this.Logger));
            }
        }

        protected void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.scope.Dispose();
            }

            this.isDisposed = true;
        }

        private void OnInverterStatusUpdated(FieldNotificationMessage message)
        {
            Debug.Assert(message.Data is InverterStatusUpdateFieldMessageData);

            if (message.DeviceIndex == (byte)this.machineData.CurrentInverterIndex)
            {
                var data = message.Data as InverterStatusUpdateFieldMessageData;

                this.positioningData.TorqueCurrentSample = data.TorqueCurrent;

                this.Logger.LogTrace($"InverterStatusUpdate inverter={this.machineData.CurrentInverterIndex}; Movement={this.positioningData.AxisMovement};");
                var notificationMessage = new NotificationMessage(
                    this.positioningData,
                    $"Current Encoder position changed",
                    MessageActor.AutomationService,
                    MessageActor.DeviceManager,
                    MessageType.Positioning,
                    this.machineData.RequestingBay,
                    this.machineData.TargetBay,
                    MessageStatus.OperationExecuting);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }
        }

        private void ProcessEndPositioning()
        {
            switch (this.positioningData.MovementMode)
            {
                case MovementMode.ExtBayChain:
                    {
                        var machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                        var distance = Math.Abs(this.positioningData.TargetPosition);
                        if (distance > 50)
                        {
                            machineProvider.UpdateBayChainStatistics(distance, this.machineData.RequestingBay);
                        }

                        this.ParentStateMachine.ChangeState(new HomingEndState(this.stateData, this.Logger));
                    }
                    break;
            }
        }

        private void ProcessEndStop()
        {
            if (/*this.machineData.MachineSensorStatus.IsSensorZeroOnCradle ||
                this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle ||*/
                this.positioningData.MovementMode == MovementMode.ExtBayChain
                )
            {
                this.ParentStateMachine.ChangeState(new HomingEndState(this.stateData, this.Logger));
            }
        }

        #endregion
    }
}
