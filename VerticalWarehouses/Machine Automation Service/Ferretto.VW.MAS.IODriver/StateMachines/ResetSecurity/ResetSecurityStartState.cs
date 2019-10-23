﻿using System;
using System.Linq;
using System.Timers;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.IODriver.Interface;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.IODriver.StateMachines.ResetSecurity
{
    public sealed class ResetSecurityStartState : IoStateBase, IDisposable
    {
        #region Fields

        private const int ResponseTimeoutMilliseconds = 2000;

        private readonly IoIndex index;

        private readonly IoStatus mainIoDevice;

        private readonly Timer responseTimer = new Timer(ResponseTimeoutMilliseconds) { AutoReset = false };

        private readonly IoStatus status;

        private bool isDisposed = false;

        #endregion

        #region Constructors

        public ResetSecurityStartState(
            IIoStateMachine parentStateMachine,
            IoStatus status,
            IoStatus mainIoDevice,
            IoIndex index,
            ILogger logger)
            : base(parentStateMachine, logger)
        {
            this.status = status ?? throw new System.ArgumentNullException(nameof(status));
            this.mainIoDevice = mainIoDevice ?? throw new System.ArgumentNullException(nameof(mainIoDevice));
            this.index = index;
            this.responseTimer.Elapsed += this.OnResponseTimedOut;
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);
        }

        public override void ProcessMessage(IoMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Reset Security={message.ResetSecurity}");

            if (message.ValidOutputs && !message.ResetSecurity)
            {
                this.ParentStateMachine.ChangeState(
                    new ResetSecurityEndState(this.ParentStateMachine, this.status, this.index, hasError: false, this.Logger));
            }
        }

        public override void ProcessResponseMessage(IoReadMessage message)
        {
            this.Logger.LogTrace($"1:Valid Outputs={message.ValidOutputs}:Reset Security={message.ResetSecurity}");

            var checkMessage =
                message.FormatDataOperation == Enumerations.ShdFormatDataOperation.Data
                &&
                message.ValidOutputs
                &&
                !message.ResetSecurity;

            if (checkMessage && this.status.MatchOutputs(message.Outputs) && this.mainIoDevice.NormalState)
            {
                this.responseTimer.Stop();
                this.ParentStateMachine.ChangeState(
                    new ResetSecurityEndState(this.ParentStateMachine, this.status, this.index, hasError: false, this.Logger));
            }
        }

        public override void Start()
        {
            var resetIoMessage = new IoWriteMessage();
            resetIoMessage.ResetSecurity = true;
            resetIoMessage.PowerEnable = true;

            this.Logger.LogTrace($"1:Reset Security Pulse={resetIoMessage}");

            lock (this.status)
            {
                this.status.UpdateOutputStates(resetIoMessage.Outputs);
            }

            this.ParentStateMachine.EnqueueMessage(resetIoMessage);

            this.responseTimer.Start();
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
            this.Logger.LogWarning("Reset security failed because there was a timeout.");

            this.ParentStateMachine.ChangeState(
                new ResetSecurityEndState(this.ParentStateMachine, this.status, this.index, hasError: true, this.Logger));
        }

        #endregion
    }
}
