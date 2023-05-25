using System;
using System.Timers;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.IODriver.StateMachines.SwitchAxis
{
    internal sealed class SwitchAxisSwitchOnMotorState : IoStateBase, IDisposable
    {
        #region Fields

        private readonly Axis axisToSwitchOn;

        private readonly BayNumber bayNumber;

        private readonly IErrorsProvider errorProvider;

        private readonly IoIndex index;

        private readonly IMachineProvider machineProvider;

        private readonly int ResponseTimeoutMilliseconds = 5000;

        private readonly Timer responseTimer;

        private readonly IoStatus status;

        private bool isDisposed = false;

        private DateTime startTime;

        #endregion

        #region Constructors

        public SwitchAxisSwitchOnMotorState(
            Axis axisToSwitchOn,
            IoStatus status,
            IoIndex index,
            BayNumber bayNumber,
            ILogger logger,
            IIoStateMachine parentStateMachine)
            : base(parentStateMachine, logger)
        {
            this.machineProvider = this.ParentStateMachine.GetRequiredService<IMachineProvider>();
            this.ResponseTimeoutMilliseconds = this.machineProvider.GetResponseTimeoutMilliseconds();
            this.responseTimer = new Timer(this.ResponseTimeoutMilliseconds) { AutoReset = false };

            this.axisToSwitchOn = axisToSwitchOn;
            this.status = status;
            this.index = index;
            this.bayNumber = bayNumber;
            this.responseTimer.Elapsed += this.OnResponseTimedOut;
            this.errorProvider = this.ParentStateMachine.GetRequiredService<IErrorsProvider>();

            logger.LogTrace("1:Method Start");
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace("1:Method Start");

            var checkMessage = message.FormatDataOperation == ShdFormatDataOperation.Data &&
                               message.ValidOutputs;

            if (this.status.MatchOutputs(message.Outputs) && checkMessage)
            {
                this.Logger.LogTrace($"2:Axis to switch on={this.axisToSwitchOn}:Cradle motor on={message.CradleMotorOn}:Elevator motor on={message.ElevatorMotorOn}");

                if ((this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn)
                    ||
                    (this.axisToSwitchOn == Axis.Vertical && message.ElevatorMotorOn))
                {
                    this.Logger.LogTrace("2b:Axis switched on");

                    var feedback = (this.axisToSwitchOn == Axis.Horizontal && message.CradleMotorOn) ? this.status.InputData[(int)IoPorts.CradleMotorFeedback] : this.status.InputData[(int)IoPorts.ElevatorMotorFeedback];
                    if (DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds > this.ResponseTimeoutMilliseconds
                        || feedback)
                    {
                        this.Logger.LogDebug($"3:Change State to EndState: feedback {feedback}, delay {DateTime.UtcNow.Subtract(this.startTime).TotalMilliseconds:0.0000}");
                        if (!feedback)
                        {
                            this.errorProvider.RecordNew(MachineErrorCode.IoDeviceCommandTimeout, this.bayNumber, additionalText: $"Switch axis motor Index {this.index}");
                        }
                        this.ParentStateMachine.ChangeState(new SwitchAxisEndState(this.axisToSwitchOn, this.status, this.index, !feedback, this.Logger, this.ParentStateMachine));
                    }
                }
            }
        }

        public override void Start()
        {
            var switchOnAxisIoMessage = new IoWriteMessage { BayLightOn = this.status.OutputData?[(int)IoPorts.BayLight] ?? false };

            this.Logger.LogTrace($"1:Switch on axis {this.axisToSwitchOn}. IO={switchOnAxisIoMessage}");

            switch (this.axisToSwitchOn)
            {
                case Axis.Horizontal:
                    switchOnAxisIoMessage.SwitchCradleMotor(true);
                    break;

                case Axis.Vertical:
                    switchOnAxisIoMessage.SwitchElevatorMotor(true);
                    break;
            }

            switchOnAxisIoMessage.ResetSecurity = false;
            switchOnAxisIoMessage.PowerEnable = true;

            this.startTime = DateTime.UtcNow;

            this.Logger.LogDebug($"2:Switch on axis {this.axisToSwitchOn}. IO={switchOnAxisIoMessage}");
            lock (this.status)
            {
                this.status.UpdateOutputStates(switchOnAxisIoMessage.Outputs);
            }

            this.ParentStateMachine.EnqueueMessage(switchOnAxisIoMessage);
        }

        private void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    this.responseTimer.Dispose();
                }

                this.isDisposed = true;
            }
        }

        private void OnResponseTimedOut(object sender, ElapsedEventArgs e)
        {
            this.Logger.LogError("Switch axis motor command timeout.");
            this.errorProvider.RecordNew(MachineErrorCode.IoDeviceCommandTimeout, this.bayNumber, additionalText: $"Switch axis motor Index {this.index}");

            this.ParentStateMachine.ChangeState(
                new SwitchAxisEndState(this.axisToSwitchOn, this.status, this.index, hasError: true, this.Logger, this.ParentStateMachine));
        }

        #endregion
    }
}
