using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.HorizontalResolution;
using Ferretto.VW.MAS.DeviceManager.HorizontalResolution.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.StateMachines.HorizontalResolution
{
    internal class HorizontalResolutionExecutingState : StateBase, IDisposable
    {
        #region Fields

        private const int DefaultStatusWordPollingInterval = 100;

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly IHorizontalResolutionMachineData machineData;

        //private readonly double[] findZeroPosition = new double[(int)HorizontalCalibrationStep.FindCenter];
        private readonly IServiceScope scope;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        //private readonly double secondPosition;
        private readonly IHorizontalResolutionStateData stateData;

        //private HorizontalCalibrationStep findZeroStep;

        private double horizontalStartingPosition;

        private bool isDisposed;

        private bool isTestStopped;

        private int performedCycles;

        private double targetPosition;

        #endregion

        #region Constructors

        public HorizontalResolutionExecutingState(IHorizontalResolutionStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IHorizontalResolutionMachineData;

            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();

            this.elevatorProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorProvider>();
            this.setupProceduresDataProvider = this.scope.ServiceProvider.GetRequiredService<ISetupProceduresDataProvider>();
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
            switch (message.Type)
            {
                case MessageType.StopTest:
                    if (this.machineData.MessageData.MovementMode == MovementMode.HorizontalResolution)
                    {
                        this.Logger.LogInformation($"Stop Horizontal Resolution on {this.machineData.RequestingBay} after {this.machineData.MessageData.ExecutedCycles} cycles");
                        this.isTestStopped = true;
                    }
                    break;

                default:
                    break;
            }
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
                            this.Logger.LogDebug($"Trace Notification Message {message.Type}");
                            this.ProcessEndPositioning();
                            break;

                        case FieldMessageType.InverterStop:
                            this.ProcessEndStop();
                            break;
                    }
                    break;

                case MessageStatus.OperationError:
                    this.stateData.FieldMessage = message;
                    // stop timers
                    this.ParentStateMachine.ChangeState(new HorizontalResolutionErrorState(this.stateData, this.Logger));
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
            FieldCommandMessage commandMessage = null;
            var inverterIndex = (byte)this.machineData.CurrentInverterIndex;

            var statusWordPollingInterval = DefaultStatusWordPollingInterval;

            switch (this.machineData.MessageData.MovementMode)
            {
                case MovementMode.HorizontalResolution:
                    {
                        var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);
                        this.targetPosition = this.machineData.MessageData.TargetPosition;
                        var nextPosition = this.elevatorProvider.HorizontalPosition;
                        if (this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards)
                        {
                            nextPosition += this.targetPosition;
                        }
                        else
                        {
                            nextPosition -= this.targetPosition;
                        }
                        positioningFieldMessageData.TargetPosition = nextPosition;
                        statusWordPollingInterval = 500;

                        commandMessage = new FieldCommandMessage(
                            positioningFieldMessageData,
                            $"External {this.machineData.MessageData.AxisMovement} Positioning State Started",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.DeviceManager,
                            FieldMessageType.Positioning,
                            inverterIndex);

                        var procedure = this.setupProceduresDataProvider.GetHorizontalResolutionCalibration();
                        this.performedCycles = procedure.PerformedCycles;

                        this.Logger.LogInformation($"Start Horizontal Resolution {this.performedCycles} cycle to {this.machineData.MessageData.RequiredCycles}");
                    }
                    break;

                default:
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }

                    break;
            }

            this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

            this.ParentStateMachine.PublishFieldCommandMessage(
                new FieldCommandMessage(
                    new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, true, statusWordPollingInterval),
                "Update Inverter status word status",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.InverterSetTimer,
                inverterIndex));
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"1:Stop Method Start. Reason {reason}");

            // stop timers

            this.stateData.StopRequestReason = reason;

            if (reason == StopRequestReason.Error)
            {
                this.ParentStateMachine.ChangeState(new HorizontalResolutionErrorState(this.stateData, this.Logger));
            }
            else
            {
                this.ParentStateMachine.ChangeState(new HorizontalResolutionEndState(this.stateData, this.Logger));
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

                this.machineData.MessageData.TorqueCurrentSample = data.TorqueCurrent;

                this.Logger.LogTrace($"InverterStatusUpdate inverter={this.machineData.CurrentInverterIndex}; Movement={this.machineData.MessageData.AxisMovement};");
                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
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
            switch (this.machineData.MessageData.MovementMode)
            {
                case MovementMode.HorizontalResolution:
                    {
                        var machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>();

                        var machineModeProvider = this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
                        if (machineModeProvider.Mode != MachineMode.Test &&
                            machineModeProvider.Mode != MachineMode.Test2 &&
                            machineModeProvider.Mode != MachineMode.Test3)
                        {
                            switch (this.machineData.TargetBay)
                            {
                                case BayNumber.BayOne:
                                    machineModeProvider.Mode = MachineMode.Test;
                                    break;

                                case BayNumber.BayTwo:
                                    machineModeProvider.Mode = MachineMode.Test2;
                                    break;

                                case BayNumber.BayThree:
                                    machineModeProvider.Mode = MachineMode.Test3;
                                    break;

                                default:
                                    machineModeProvider.Mode = MachineMode.Test;
                                    break;
                            }

                            this.Logger.LogInformation($"Machine status switched to {machineModeProvider.Mode}");
                        }

                        // Update the setup procedure data
                        var procedure = this.setupProceduresDataProvider.GetHorizontalResolutionCalibration();
                        this.performedCycles = this.setupProceduresDataProvider.IncreasePerformedCycles(procedure).PerformedCycles;
                        this.machineData.MessageData.ExecutedCycles = this.performedCycles;

                        if (!this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                        {
                            this.Logger.LogError($"Invalid sensors condition. An error occurs");

                            this.Stop(StopRequestReason.Error);
                            break;
                        }
                        else
                        {
                            if (this.performedCycles >= this.machineData.MessageData.RequiredCycles ||
                                this.isTestStopped)
                            {
                                this.Logger.LogDebug("FSM Finished Executing State");
                                this.machineData.ExecutedSteps = this.performedCycles;
                                this.machineData.MessageData.IsTestStopped = this.isTestStopped;

                                this.ParentStateMachine.ChangeState(new HorizontalResolutionEndState(this.stateData, this.Logger));
                                break;
                            }
                            else
                            {
                                this.Logger.LogInformation($"Start another Horizontal Resolution after {this.performedCycles} cycles to {this.machineData.MessageData.RequiredCycles}");
                                var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);
                                var nextPosition = this.elevatorProvider.HorizontalPosition;
                                if (this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards)
                                {
                                    nextPosition += this.targetPosition;
                                }
                                else
                                {
                                    nextPosition -= this.targetPosition;
                                }
                                positioningFieldMessageData.TargetPositionOriginal = nextPosition;
                                var inverterIndex = (byte)this.machineData.CurrentInverterIndex;

                                var commandMessage = new FieldCommandMessage(
                                    positioningFieldMessageData,
                                    $"External {this.machineData.MessageData.AxisMovement} Positioning State Started",
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageActor.DeviceManager,
                                    FieldMessageType.Positioning,
                                    inverterIndex);
                                this.ParentStateMachine.PublishFieldCommandMessage(commandMessage);

                                this.ParentStateMachine.PublishFieldCommandMessage(
                                    new FieldCommandMessage(
                                        new InverterSetTimerFieldMessageData(InverterTimer.StatusWord, true, DefaultStatusWordPollingInterval),
                                    "Update Inverter status word status",
                                    FieldMessageActor.InverterDriver,
                                    FieldMessageActor.DeviceManager,
                                    FieldMessageType.InverterSetTimer,
                                    inverterIndex));

                                var notificationMessage = new NotificationMessage(
                                    this.machineData.MessageData,
                                    $"BayTest {this.machineData.ExecutedSteps} / {this.machineData.MessageData.RequiredCycles}",
                                    MessageActor.AutomationService,
                                    MessageActor.DeviceManager,
                                    MessageType.Positioning,
                                    this.machineData.RequestingBay,
                                    this.machineData.TargetBay,
                                    MessageStatus.OperationExecuting);

                                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                            }
                        }
                    }
                    break;
            }
        }

        private void ProcessEndStop()
        {
            if (this.machineData.MessageData.MovementMode == MovementMode.HorizontalResolution)
            {
                this.machineData.ExecutedSteps = this.performedCycles;

                // stop timers
                this.ParentStateMachine.ChangeState(new HorizontalResolutionEndState(this.stateData, this.Logger));
            }
        }

        #endregion
    }
}
