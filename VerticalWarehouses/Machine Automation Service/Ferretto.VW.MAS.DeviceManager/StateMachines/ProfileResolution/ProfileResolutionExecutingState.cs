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
using Ferretto.VW.MAS.DeviceManager.ProfileResolution;
using Ferretto.VW.MAS.DeviceManager.ProfileResolution.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.StateMachines.ProfileResolution
{
    internal class ProfileResolutionExecutingState : StateBase, IDisposable
    {
        #region Fields

        private const int DefaultStatusWordPollingInterval = 100;

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly IProfileResolutionMachineData machineData;

        private readonly int[] profile;

        private readonly IServiceScope scope;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly IProfileResolutionStateData stateData;

        private double eightBeamPosition;

        private bool isDisposed;

        private bool isTestStopped;

        private bool measureRequest;

        private int performedCycles;

        private Timer profileTimerIO;

        private Timer profileTimerInverter;

        private double targetPosition;

        private double thirtyBeamPosition;

        #endregion

        #region Constructors

        public ProfileResolutionExecutingState(IProfileResolutionStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IProfileResolutionMachineData;

            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();

            this.elevatorProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorProvider>();
            this.setupProceduresDataProvider = this.scope.ServiceProvider.GetRequiredService<ISetupProceduresDataProvider>();
            this.errorsProvider = this.scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.baysDataProvider = this.scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();

            this.profile = new int[(int)ProfileResolutionStep.ThirtyBeam + 1];
            this.measureRequest = false;

            this.profileTimerIO = new Timer(this.RequestMeasureProfileIO, null, Timeout.Infinite, Timeout.Infinite);
            this.profileTimerInverter = new Timer(this.RequestMeasureProfileInverter, null, Timeout.Infinite, Timeout.Infinite);
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
                    if (this.machineData.MessageData.MovementMode == MovementMode.ProfileResolution)
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

                        case FieldMessageType.MeasureProfile:
                            this.ProcessEndMeasure(message);
                            break;

                        case FieldMessageType.InverterStop:
                            this.ProcessEndStop();
                            break;
                    }
                    break;

                case MessageStatus.OperationError:
                    this.stateData.FieldMessage = message;
                    // stop timers
                    this.profileTimerIO?.Change(Timeout.Infinite, Timeout.Infinite);
                    this.profileTimerInverter?.Change(Timeout.Infinite, Timeout.Infinite);
                    this.ParentStateMachine.ChangeState(new ProfileResolutionErrorState(this.stateData, this.Logger));
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
                case MovementMode.ProfileResolution:
                    {
                        var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);
                        this.targetPosition = this.machineData.MessageData.TargetPosition;
                        positioningFieldMessageData.TargetPosition = this.targetPosition;
                        var bay = this.baysDataProvider.GetByNumber(this.machineData.RequestingBay);
                        var bayPosition = new BayPosition();
                        if (!bay.IsDouble)
                        {
                            bayPosition = bay.Positions.First();
                        }
                        else if (bay.Carousel != null)
                        {
                            bayPosition = bay.Positions.First(p => p.IsUpper);
                        }
                        else
                        {
                            bayPosition = bay.Positions.First(p => !p.IsUpper);
                        }
                        this.eightBeamPosition = bayPosition.Height + 160;
                        this.thirtyBeamPosition = bayPosition.Height + 780;
                        statusWordPollingInterval = 500;

                        commandMessage = new FieldCommandMessage(
                            positioningFieldMessageData,
                            $"{this.machineData.MessageData.AxisMovement} Positioning State Started",
                            FieldMessageActor.InverterDriver,
                            FieldMessageActor.DeviceManager,
                            FieldMessageType.Positioning,
                            inverterIndex);

                        this.Logger.LogInformation($"Start Profile Resolution step [{this.performedCycles}] to {positioningFieldMessageData.TargetPosition:0.00}");
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

            var notificationMessage = new NotificationMessage(
                this.machineData.MessageData,
                $"ProfileResolution",
                MessageActor.AutomationService,
                MessageActor.DeviceManager,
                MessageType.Positioning,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationExecuting);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"1:Stop Method Start. Reason {reason}");

            // stop timers
            this.profileTimerIO?.Change(Timeout.Infinite, Timeout.Infinite);
            this.profileTimerInverter?.Change(Timeout.Infinite, Timeout.Infinite);

            this.stateData.StopRequestReason = reason;

            if (reason == StopRequestReason.Error)
            {
                this.ParentStateMachine.ChangeState(new ProfileResolutionErrorState(this.stateData, this.Logger));
            }
            else
            {
                this.ParentStateMachine.ChangeState(new ProfileResolutionEndState(this.stateData, this.Logger));
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
                this.profileTimerIO?.Dispose();
                this.profileTimerInverter?.Dispose();
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

        /// <summary>
        /// this simply applies the "line" formula:
        /// y - y0      x - x0
        /// ------   =  ------
        /// y1 - y0     x1 - x0
        ///
        /// or also y = k1 * x + k0
        ///
        /// where
        ///
        /// k1 = (y1 - y0) / (x1 - x0)
        /// k0 = y0 - x0 * k1
        /// </summary>
        private void ParametersCalculation()
        {
            //// test
            //this.profile[0] = 2020;
            //this.profile[2] = 10083;
            ////end test
            var k1 = (double)(774 - 24) / (this.profile[(int)ProfileResolutionStep.ThirtyBeam] - this.profile[(int)ProfileResolutionStep.ZeroBeam]);
            var k0 = 24 - this.profile[(int)ProfileResolutionStep.ZeroBeam] * k1 - 49;
            this.machineData.MessageData.ProfileConst = new double[2];
            this.machineData.MessageData.ProfileConst[0] = k0;
            this.machineData.MessageData.ProfileConst[1] = k1;

            this.Logger.LogDebug($"Profile constants: k0 {k0:0.00}; k1 {k1:0.0000}");
        }

        private void ProcessEndMeasure(FieldNotificationMessage message)
        {
            if (message.Data is MeasureProfileFieldMessageData data && message.Source == FieldMessageActor.InverterDriver)
            {
                this.Logger.LogDebug($"Step {this.performedCycles} Profile {data.Profile / 100.0}%");
                this.profile[this.performedCycles] = data.Profile;
                this.StartNewStep();
            }
            else if (message.Source == FieldMessageActor.IoDriver && this.measureRequest)
            {
                this.profileTimerInverter = new Timer(this.RequestMeasureProfileInverter, null, 600, 600);
            }
        }

        private void ProcessEndPositioning()
        {
            switch (this.machineData.MessageData.MovementMode)
            {
                case MovementMode.ProfileResolution:
                    {
                        var machineModeProvider = this.scope.ServiceProvider.GetRequiredService<IMachineVolatileDataProvider>();
                        if (machineModeProvider.Mode != MachineMode.Test &&
                            machineModeProvider.Mode != MachineMode.Test2 &&
                            machineModeProvider.Mode != MachineMode.Test3)
                        {
                            switch (this.machineData.RequestingBay)
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

                        var ioCommandMessageData = new MeasureProfileFieldMessageData(true);
                        var ioCommandMessage = new FieldCommandMessage(
                            ioCommandMessageData,
                            $"Measure Profile Start ",
                            FieldMessageActor.IoDriver,
                            FieldMessageActor.DeviceManager,
                            FieldMessageType.MeasureProfile,
                            (byte)this.baysDataProvider.GetIoDevice(this.machineData.RequestingBay));

                        this.Logger.LogTrace($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

                        this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);

                        this.measureRequest = false;

                        this.profileTimerIO = new Timer(this.RequestMeasureProfileIO, null, 600, 600);
                    }
                    break;
            }
        }

        private void ProcessEndStop()
        {
            if (this.machineData.MessageData.MovementMode == MovementMode.ProfileResolution)
            {
                // stop timers
                this.profileTimerIO?.Change(Timeout.Infinite, Timeout.Infinite);
                this.profileTimerInverter?.Change(Timeout.Infinite, Timeout.Infinite);
                this.ParentStateMachine.ChangeState(new ProfileResolutionEndState(this.stateData, this.Logger));
            }
        }

        private void RequestMeasureProfileIO(object state)
        {
            this.Logger.LogTrace($"Request MeasureProfile IO");
            var ioCommandMessageData = new MeasureProfileFieldMessageData(false);
            var ioCommandMessage = new FieldCommandMessage(
                ioCommandMessageData,
                $"Measure Profile Start ",
                FieldMessageActor.IoDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.MeasureProfile,
                (byte)this.baysDataProvider.GetIoDevice(this.machineData.RequestingBay));

            this.Logger.LogTrace($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);

            this.measureRequest = true;

            // suspend timer at every call
            this.profileTimerIO?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void RequestMeasureProfileInverter(object state)
        {
            this.Logger.LogTrace($"Request MeasureProfile Inverter");
            var inverterIndex = (byte)this.baysDataProvider.GetInverterIndexByProfile(this.machineData.RequestingBay);
            var inverterCommandMessageData = new MeasureProfileFieldMessageData();
            var inverterCommandMessage = new FieldCommandMessage(
                inverterCommandMessageData,
                $"Measure Profile",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.MeasureProfile,
                inverterIndex);

            //this.Logger.LogTrace($"5:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

            // suspend timer at every call
            this.profileTimerInverter?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void StartNewStep()
        {
            var errorText = string.Empty;
            if (this.performedCycles == (int)ProfileResolutionStep.ZeroBeam
                && this.profile[this.performedCycles] > 2200)
            {
                errorText = Resources.ResolutionCalibrationProcedure.ResourceManager.GetString("ProfileResolutionTooHigh", CommonUtils.Culture.Actual);
            }
            else if (this.performedCycles == (int)ProfileResolutionStep.EightBeam
                && !this.machineData.MachineSensorStatus.IsProfileCalibratedBay(this.machineData.RequestingBay))
            {
                //errorText = Resources.ResolutionCalibrationProcedure.ResourceManager.GetString("ProfileResolutionMissingSignal", CommonUtils.Culture.Actual);
                this.Logger.LogError(Resources.ResolutionCalibrationProcedure.ResourceManager.GetString("ProfileResolutionMissingSignal", CommonUtils.Culture.Actual));
            }
            else if (this.performedCycles == (int)ProfileResolutionStep.ThirtyBeam
                && this.profile[this.performedCycles] < 9800)
            {
                errorText = Resources.ResolutionCalibrationProcedure.ResourceManager.GetString("ProfileResolutionTooLow", CommonUtils.Culture.Actual);
            }
            if (errorText.Length > 0)
            {
#if DEBUG
                this.Logger.LogError(errorText);
#else
                this.errorsProvider.RecordNew(MachineErrorCode.ProfileResolutionFail, this.machineData.RequestingBay, errorText);
                this.isTestStopped = true;
#endif
            }

            if (++this.performedCycles >= this.machineData.MessageData.RequiredCycles
                || this.isTestStopped)
            {
                this.Logger.LogDebug("FSM Finished Executing State");
                this.machineData.ExecutedSteps = this.performedCycles;
                this.machineData.MessageData.ExecutedCycles = this.performedCycles;
                this.machineData.MessageData.IsTestStopped = this.isTestStopped;
                this.machineData.MessageData.ProfileSamples = this.profile;
                if (this.isTestStopped)
                {
                    this.machineData.ExecutedSteps = this.performedCycles;
                    this.machineData.MessageData.ExecutedCycles = this.performedCycles;
                    this.machineData.MessageData.ProfileSamples = this.profile;
                    var notificationMessage = new NotificationMessage(
                        this.machineData.MessageData,
                        $"ProfileResolution",
                        MessageActor.AutomationService,
                        MessageActor.DeviceManager,
                        MessageType.Positioning,
                        this.machineData.RequestingBay,
                        this.machineData.TargetBay,
                        MessageStatus.OperationExecuting);

                    this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
                }
                else
                {
                    this.ParametersCalculation();
                }

                // stop timers
                this.profileTimerIO?.Change(Timeout.Infinite, Timeout.Infinite);
                this.profileTimerInverter?.Change(Timeout.Infinite, Timeout.Infinite);
                this.ParentStateMachine.ChangeState(new ProfileResolutionEndState(this.stateData, this.Logger));
            }
            else
            {
                var positioningFieldMessageData = new PositioningFieldMessageData(this.machineData.MessageData, this.machineData.RequestingBay);
                if (this.performedCycles == (int)ProfileResolutionStep.EightBeam)
                {
                    positioningFieldMessageData.TargetPosition = this.eightBeamPosition;
                }
                else
                {
                    positioningFieldMessageData.TargetPosition = this.thirtyBeamPosition;
                }
                positioningFieldMessageData.TargetPositionOriginal = positioningFieldMessageData.TargetPosition;
                this.Logger.LogInformation($"Start Profile Resolution step [{this.performedCycles}] to {positioningFieldMessageData.TargetPosition:0.00}");
                var inverterIndex = (byte)this.machineData.CurrentInverterIndex;

                var commandMessage = new FieldCommandMessage(
                    positioningFieldMessageData,
                    $"{this.machineData.MessageData.AxisMovement} Positioning State Started",
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

                this.machineData.ExecutedSteps = this.performedCycles;
                this.machineData.MessageData.ExecutedCycles = this.performedCycles;
                this.machineData.MessageData.ProfileSamples = this.profile;
                var notificationMessage = new NotificationMessage(
                    this.machineData.MessageData,
                    $"ProfileResolution",
                    MessageActor.AutomationService,
                    MessageActor.DeviceManager,
                    MessageType.Positioning,
                    this.machineData.RequestingBay,
                    this.machineData.TargetBay,
                    MessageStatus.OperationExecuting);

                this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
            }
        }

        #endregion
    }
}
