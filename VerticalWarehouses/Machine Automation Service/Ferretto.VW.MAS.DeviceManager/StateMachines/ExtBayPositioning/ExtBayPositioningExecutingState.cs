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
using Ferretto.VW.MAS.DeviceManager.ExtBayPositioning;
using Ferretto.VW.MAS.DeviceManager.ExtBayPositioning.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.StateMachines.ExtBayPositioning
{
    internal class ExtBayPositioningExecutingState : StateBase, IDisposable
    {
        #region Fields

        private const int DefaultStatusWordPollingInterval = 100;

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly IExtBayPositioningMachineData machineData;

        //private readonly double[] findZeroPosition = new double[(int)HorizontalCalibrationStep.FindCenter];
        private readonly IServiceScope scope;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        //private readonly double secondPosition;
        private readonly IExtBayPositioningStateData stateData;

        private Timer delayTimer;

        //private HorizontalCalibrationStep findZeroStep;

        private double horizontalStartingPosition;

        private bool isDisposed;

        private bool isTestStopped;

        private int performedCycles;

        private double targetPosition;

        #endregion

        #region Constructors

        public ExtBayPositioningExecutingState(IExtBayPositioningStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IExtBayPositioningMachineData;

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
                    if (this.machineData.MessageData.MovementMode == MovementMode.ExtBayTest)
                    {
                        this.Logger.LogInformation($"Stop Bay Test on {this.machineData.RequestingBay} after {this.machineData.MessageData.ExecutedCycles} cycles");
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
                    // stop timers
                    this.delayTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                    this.ParentStateMachine.ChangeState(new ExtBayPositioningErrorState(this.stateData, this.Logger));
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
            var elevatorDataProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();

            switch (this.machineData.MessageData.MovementMode)
            {
                case MovementMode.ExtBayChain:
                case MovementMode.ExtBayChainManual:
                case MovementMode.ExtBayTest:
                    {
                        var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);

                        commandMessage = new FieldCommandMessage(
                            positioningFieldMessageData,
                            $"External {this.machineData.MessageData.AxisMovement} Positioning State Started",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.DeviceManager,
                            FieldMessageType.Positioning,
                            inverterIndex);

                        if (this.machineData.MessageData.MovementMode == MovementMode.BayTest)
                        {
                            var procedure = this.setupProceduresDataProvider.GetBayExternalCalibration(this.machineData.RequestingBay);
                            this.performedCycles = procedure.PerformedCycles;
                            this.Logger.LogInformation($"Start External Bay Calibration Test {this.performedCycles} cycle to {this.machineData.MessageData.RequiredCycles}");
                        }
                    }
                    break;

                case MovementMode.HorizontalCalibration:
                    {
                        //this.findZeroStep = HorizontalCalibrationStep.LeaveZeroSensor;
                        this.horizontalStartingPosition = this.elevatorProvider.HorizontalPosition;
                        this.targetPosition = Math.Abs(this.machineData.MessageData.TargetPosition) * 0.99;
                        statusWordPollingInterval = 500;

                        var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);

                        commandMessage = new FieldCommandMessage(
                            positioningFieldMessageData,
                            $"External {this.machineData.MessageData.AxisMovement} Positioning Find Zero Started",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.DeviceManager,
                            FieldMessageType.Positioning,
                            inverterIndex);
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
            this.delayTimer?.Change(Timeout.Infinite, Timeout.Infinite);

            this.stateData.StopRequestReason = reason;

            if (reason == StopRequestReason.Error)
            {
                this.ParentStateMachine.ChangeState(new ExtBayPositioningErrorState(this.stateData, this.Logger));
            }
            else
            {
                this.ParentStateMachine.ChangeState(new ExtBayPositioningEndState(this.stateData, this.Logger));
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
                this.delayTimer?.Dispose();
                this.scope.Dispose();
            }

            this.isDisposed = true;
        }

        private void DelayExtBayElapsed(object state)
        {
            this.Logger.LogInformation($"Start another External Bay Calibration Test after {this.performedCycles} cycles to {this.machineData.MessageData.RequiredCycles}");

            // Retrieve the current external bay movement direction
            var externalBayMovementDirection = (this.machineData.MessageData.Direction == HorizontalMovementDirection.Backwards) ?
                ExternalBayMovementDirection.TowardOperator :
                ExternalBayMovementDirection.TowardMachine;

            // Reverse the direction
            if (externalBayMovementDirection == ExternalBayMovementDirection.TowardOperator)
            {
                externalBayMovementDirection = ExternalBayMovementDirection.TowardMachine;
            }
            else
            {
                externalBayMovementDirection = ExternalBayMovementDirection.TowardOperator;
            }

            // Carry out the target position
            var bay = this.baysDataProvider.GetByNumber(this.machineData.RequestingBay);
            var race = bay.External.Race;

            var distanceMovement = race;
            switch (externalBayMovementDirection)
            {
                case ExternalBayMovementDirection.TowardOperator:
                    distanceMovement = race - Math.Abs(this.baysDataProvider.GetChainPosition(this.machineData.RequestingBay)) - bay.ChainOffset;
                    break;

                case ExternalBayMovementDirection.TowardMachine:
                    distanceMovement = 0 - Math.Abs(this.baysDataProvider.GetChainPosition(this.machineData.RequestingBay)) + bay.ChainOffset;
                    break;
            }

            //var targetPosition = (externalBayMovementDirection == ExternalBayMovementDirection.TowardOperator) ? race + bay.ChainOffset : bay.ChainOffset; // for .Absolute
            var targetPosition = distanceMovement;

            // Set target parameter and direction parameter
            //this.machineData.MessageData.Direction = (externalBayMovementDirection == ExternalBayMovementDirection.TowardOperator) ?
            //    HorizontalMovementDirection.Forwards :
            //    HorizontalMovementDirection.Backwards;
            this.machineData.MessageData.Direction = (externalBayMovementDirection == ExternalBayMovementDirection.TowardOperator) ?
                HorizontalMovementDirection.Backwards :
                HorizontalMovementDirection.Forwards;
            this.machineData.MessageData.TargetPosition = targetPosition;

            var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);
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
                $"External Bay Calibration Test {this.machineData.ExecutedSteps} / {this.machineData.MessageData.RequiredCycles}",
                MessageActor.AutomationService,
                MessageActor.DeviceManager,
                MessageType.Positioning,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationExecuting);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        private bool IsBracketSensorError()
        {
            return !this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.TargetBay);
        }

        /// <summary>
        /// Check the sensor status after the current movement on external bay
        /// </summary>
        /// <returns>
        ///     <c>true</c> if invalid sensor condition occurs,
        ///     <c>false</c> otherwise.
        /// </returns>
        private bool IsInvalidSensorsCondition()
        {
            var bay = this.baysDataProvider.GetByNumber(this.machineData.RequestingBay);
            //var externalBayMovementDirection = (this.machineData.MessageData.Direction == HorizontalMovementDirection.Forwards) ?
            //    ExternalBayMovementDirection.TowardOperator :
            //    ExternalBayMovementDirection.TowardMachine;

            var externalBayMovementDirection = (this.machineData.MessageData.Direction == HorizontalMovementDirection.Backwards) ?
                ExternalBayMovementDirection.TowardOperator :
                ExternalBayMovementDirection.TowardMachine;

            var failed = false;

            switch (externalBayMovementDirection)
            {
                case ExternalBayMovementDirection.TowardOperator:
                    {
                        if (bay.IsDouble)
                        {
                            if (bay.Positions.FirstOrDefault(s => s.Id == this.machineData.MessageData.TargetBayPositionId).IsUpper)
                            {
                                failed = !this.machineData.MachineSensorStatus.IsDrawerInBayInternalTop(this.machineData.RequestingBay);
                                //failed = !this.machineData.MachineSensorStatus.IsDrawerInBayTop(this.machineData.RequestingBay);
                            }
                            else
                            {
                                failed = !this.machineData.MachineSensorStatus.IsDrawerInBayBottom(this.machineData.RequestingBay);
                            }
                        }
                        else
                        {
                            failed = !this.machineData.MachineSensorStatus.IsDrawerInBayExternalPosition(this.machineData.RequestingBay, bay.IsExternal && bay.IsDouble);
                        }
                        break;
                    }

                case ExternalBayMovementDirection.TowardMachine:
                    {
                        if (bay.IsDouble)
                        {
                            if (bay.Positions.FirstOrDefault(s => s.Id == this.machineData.MessageData.SourceBayPositionId).IsUpper)
                            {
                                failed = !this.machineData.MachineSensorStatus.IsDrawerInBayTop(this.machineData.RequestingBay);
                                //failed = !this.machineData.MachineSensorStatus.IsDrawerInBayInternalTop(this.machineData.RequestingBay);
                            }
                            else
                            {
                                failed = !this.machineData.MachineSensorStatus.IsDrawerInBayInternalBottom(this.machineData.RequestingBay);
                            }
                        }
                        else
                        {
                            failed = !this.machineData.MachineSensorStatus.IsDrawerInBayInternalPosition(this.machineData.RequestingBay, bay.IsDouble);
                        }
                        break;
                    }
            }

            return failed;
        }

        private void OnInverterStatusUpdated(FieldNotificationMessage message)
        {
            Debug.Assert(message.Data is InverterStatusUpdateFieldMessageData);

            switch (this.machineData.MessageData.MovementMode)
            {
                case MovementMode.HorizontalCalibration:
                    {
                        //if (message.DeviceIndex == (byte)this.machineData.CurrentInverterIndex)
                        //{
                        //    var elevatorDataProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
                        //    var axis = elevatorDataProvider.GetAxis(Orientation.Horizontal);

                        //    var data = message.Data as InverterStatusUpdateFieldMessageData;
                        //    var chainPosition = data.CurrentPosition;
                        //    if (chainPosition.HasValue
                        //        && Math.Abs(this.horizontalStartingPosition - chainPosition.Value) > Math.Abs(axis.ChainOffset) * 2
                        //        )
                        //    {
                        //        switch (this.findZeroStep)
                        //        {
                        //            case HorizontalCalibrationStep.LeaveZeroSensor:
                        //                if (!this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                        //                {
                        //                    this.findZeroStep++;
                        //                    this.Logger.LogInformation($"Horizontal calibration step {this.findZeroStep}, Value {chainPosition:0.0000}");
                        //                }
                        //                break;

                        //            case HorizontalCalibrationStep.ForwardFindZeroSensor:
                        //            case HorizontalCalibrationStep.BackwardFindZeroSensor:
                        //                if (this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                        //                {
                        //                    this.findZeroPosition[(int)this.findZeroStep] = chainPosition.Value;
                        //                    this.findZeroStep++;
                        //                    this.Logger.LogInformation($"Horizontal calibration step {this.findZeroStep}, Value {chainPosition:0.0000}");
                        //                }
                        //                break;

                        //            case HorizontalCalibrationStep.ForwardLeaveZeroSensor:
                        //                if (!this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                        //                {
                        //                    this.findZeroPosition[(int)this.findZeroStep] = chainPosition.Value;
                        //                    this.findZeroStep++;
                        //                    this.Logger.LogInformation($"Horizontal calibration step {this.findZeroStep}, Value {chainPosition:0.0000}");
                        //                    var invertDirection = (this.machineData.MessageData.TargetPosition > 0) ? -1 : 1;
                        //                    this.FindZeroNextPosition(Math.Abs(axis.ChainOffset) * 20 * invertDirection);
                        //                }
                        //                break;

                        //            case HorizontalCalibrationStep.BackwardLeaveZeroSensor:
                        //                if (!this.machineData.MachineSensorStatus.IsSensorZeroOnCradle)
                        //                {
                        //                    this.findZeroPosition[(int)this.findZeroStep] = chainPosition.Value;
                        //                    this.findZeroStep++;
                        //                    this.Logger.LogInformation($"Horizontal calibration step {this.findZeroStep}, Value {chainPosition:0.0000}");
                        //                    this.FindZeroNextPosition((this.findZeroPosition[(int)HorizontalCalibrationStep.ForwardLeaveZeroSensor] - chainPosition.Value) / 2);
                        //                }
                        //                break;
                        //        }
                        //    }
                        //}

                        break;
                    }

                case MovementMode.Position when this.machineData.MessageData.MovementType == MovementType.TableTarget:
                    {
                        //if (this.IsLoadingErrorDuringPickup())
                        //{
                        //    this.Logger.LogWarning("Cradle not correctly loaded during pickup");
                        //    //this.errorsProvider.RecordNew(DataModels.MachineErrorCode.CradleNotCorrectlyLoadedDuringPickup, this.machineData.RequestingBay);

                        //    //this.stateData.FieldMessage = message;
                        //    //this.Stop(StopRequestReason.Stop);
                        //}
                        //else if (this.IsUnloadingErrorDuringDeposit())
                        //{
                        //    this.Logger.LogWarning("Cradle not correctly unloaded during deposit");
                        //    //this.errorsProvider.RecordNew(DataModels.MachineErrorCode.CradleNotCorrectlyUnloadedDuringDeposit, this.machineData.RequestingBay);
                        //    //this.stateData.FieldMessage = message;
                        //    //this.Stop(StopRequestReason.Stop);
                        //}

                        ////if (this.IsHorizontalSensorsError())
                        ////{
                        ////    this.errorsProvider.RecordNew(DataModels.MachineErrorCode.InvalidPresenceSensors, this.machineData.RequestingBay);

                        ////    this.stateData.FieldMessage = message;
                        ////    this.Stop(StopRequestReason.Error);
                        ////}
                        break;
                    }

                case MovementMode.Position when this.machineData.MessageData.MovementType == MovementType.Absolute:
                case MovementMode.Position when this.machineData.MessageData.MovementType == MovementType.Relative:
                case MovementMode.PositionAndMeasureProfile:
                case MovementMode.PositionAndMeasureWeight:
                    {
                        //if (!this.machineData.MessageData.BypassConditions
                        //    && this.IsSensorsError(this.machineData.MessageData.AxisMovement)
                        //    )
                        //{
                        //    this.errorsProvider.RecordNew(DataModels.MachineErrorCode.InvalidPresenceSensors, this.machineData.RequestingBay);

                        //    this.stateData.FieldMessage = message;
                        //    this.Stop(StopRequestReason.Error);
                        //}
                        break;
                    }

                case MovementMode.ProfileCalibration:
                    {
                        //if (message.DeviceIndex == (byte)this.machineData.CurrentInverterIndex)
                        //{
                        //    var data = message.Data as InverterStatusUpdateFieldMessageData;
                        //    var chainPosition = data.CurrentPosition;

                        //    var machineResourcesProvider = this.scope.ServiceProvider.GetRequiredService<IMachineResourcesProvider>();
                        //    if (machineResourcesProvider.IsProfileCalibratedBay(this.machineData.RequestingBay))
                        //    {
                        //        if (this.countProfileCalibrated == 0
                        //            && !this.profileStartPosition.HasValue)
                        //        {
                        //            this.profileStartPosition = chainPosition;
                        //            this.Logger.LogInformation($"profileStartPosition = {this.profileStartPosition.Value:0.0000}");
                        //        }
                        //        else if (this.countProfileCalibrated == 1
                        //            && !this.profileCalibratePosition.HasValue)
                        //        {
                        //            if (chainPosition.HasValue && this.profileStartPosition.HasValue)
                        //            {
                        //                this.profileCalibratePosition = data.CurrentPosition;
                        //            }

                        //            this.Logger.LogInformation($"profileCalibratePosition reached! Value {this.profileCalibratePosition.Value:0.0000}");
                        //            this.ReturnToStartPosition();
                        //            this.countProfileCalibrated = 2;
                        //        }
                        //    }
                        //    else if (this.countProfileCalibrated == 0
                        //        && this.profileStartPosition.HasValue)
                        //    {
                        //        // profileCalibrated signal is low after startPosition
                        //        this.countProfileCalibrated = 1;
                        //    }
                        //    else if (chainPosition.HasValue
                        //        && this.countProfileCalibrated < 2
                        //        && Math.Abs(chainPosition.Value - this.horizontalStartingPosition) >= this.targetPosition
                        //        )
                        //    {
                        //        this.countProfileCalibrated = 2;
                        //        this.Logger.LogInformation($"profileCalibratePosition NOT reached!");
                        //        this.ReturnToStartPosition();
                        //    }
                        //}
                        break;
                    }

                case MovementMode.BeltBurnishing:
                    //if (this.IsSensorsError(this.machineData.MessageData.AxisMovement))
                    //{
                    //    this.errorsProvider.RecordNew(DataModels.MachineErrorCode.InvalidPresenceSensors, this.machineData.RequestingBay);

                    //    this.stateData.FieldMessage = message;
                    //    this.Stop(StopRequestReason.Error);
                    //}
                    break;
            }

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
                case MovementMode.ExtBayChain:
                    {
                        var machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                        var distance = Math.Abs(this.machineData.MessageData.TargetPosition);
                        if (distance > 50)
                        {
                            machineProvider.UpdateBayChainStatistics(distance, this.machineData.RequestingBay);
                        }

                        if (!this.machineData.MessageData.BypassConditions &&
                            this.IsInvalidSensorsCondition())
                        {
                            this.Logger.LogError($"Invalid sensors condition. An error occurs");
                            this.errorsProvider.RecordNew(DataModels.MachineErrorCode.MoveExtBayNotAllowed, this.machineData.RequestingBay);
                            this.Stop(StopRequestReason.Stop);
                        }
                        else
                        {
                            this.ParentStateMachine.ChangeState(new ExtBayPositioningEndState(this.stateData, this.Logger));
                        }
                    }
                    break;

                case MovementMode.ExtBayChainManual:
                    this.ParentStateMachine.ChangeState(new ExtBayPositioningEndState(this.stateData, this.Logger));
                    break;

                case MovementMode.ExtBayTest:
                    {
                        var machineProvider = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                        var distance = Math.Abs(this.machineData.MessageData.TargetPosition);
                        if (distance > 50)
                        {
                            machineProvider.UpdateBayChainStatistics(distance, this.machineData.RequestingBay);
                        }

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
                        var procedure = this.setupProceduresDataProvider.GetBayExternalCalibration(this.machineData.RequestingBay);
                        this.performedCycles = this.setupProceduresDataProvider.IncreasePerformedCycles(procedure).PerformedCycles;
                        this.machineData.MessageData.ExecutedCycles = this.performedCycles;

                        MessageStatus status;
                        var externalBayMovementDirection = (this.machineData.MessageData.Direction == HorizontalMovementDirection.Backwards) ?
                            ExternalBayMovementDirection.TowardOperator :
                            ExternalBayMovementDirection.TowardMachine;
                        bool failed;
                        if (externalBayMovementDirection == ExternalBayMovementDirection.TowardOperator)
                        {
                            failed = this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.RequestingBay);
                        }
                        else
                        {
                            failed = !this.machineData.MachineSensorStatus.IsSensorZeroOnBay(this.machineData.RequestingBay);
                        }

                        if (failed)
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

                                this.ParentStateMachine.ChangeState(new ExtBayPositioningEndState(this.stateData, this.Logger));
                                break;
                            }
                            else
                            {
                                if (this.machineData.MessageData.Delay > 0)
                                {
                                    this.delayTimer = new Timer(this.DelayExtBayElapsed, null, this.machineData.MessageData.Delay * 1000, Timeout.Infinite);
                                }
                                else
                                {
                                    this.DelayExtBayElapsed(null);
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private void ProcessEndStop()
        {
            if (/*this.machineData.MachineSensorStatus.IsSensorZeroOnCradle ||
                this.machineData.MachineSensorStatus.IsDrawerCompletelyOnCradle ||*/
                this.machineData.MessageData.MovementMode == MovementMode.BayChain ||
                this.machineData.MessageData.MovementMode == MovementMode.BayChainManual
                )
            {
                this.machineData.ExecutedSteps = this.performedCycles;

                // stop timers
                this.delayTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                this.ParentStateMachine.ChangeState(new ExtBayPositioningEndState(this.stateData, this.Logger));
            }
        }

        #endregion
    }
}
