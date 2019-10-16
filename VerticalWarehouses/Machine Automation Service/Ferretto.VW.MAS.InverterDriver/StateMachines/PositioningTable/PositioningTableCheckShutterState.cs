using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

// TODO remove this unused class

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningTableCheckShutterState : InverterStateBase
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        private readonly AglInverterStatus inverterShutter;

        private readonly Timer shutterCheckTimer;

        #endregion

        #region Constructors

        public PositioningTableCheckShutterState(
            IInverterStateMachine parentStateMachine,
            IInverterPositioningFieldMessageData data,
            IPositioningInverterStatus inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            this.data = data;

            this.Inverter = inverterStatus;
            this.shutterCheckTimer = new Timer(this.ShutterCheck, null, -1, Timeout.Infinite);

            var invertersProvider = this.ParentStateMachine.GetRequiredService<IInvertersProvider>();
            var inverter = invertersProvider.GetShutterInverter(data.RequestingBay);
            if (inverter is AglInverterStatus)
            {
                this.inverterShutter = (AglInverterStatus)inverter;
            }
        }

        #endregion

        #region Properties

        public IPositioningInverterStatus Inverter { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogDebug("Inverter Check Shutter");
            this.shutterCheckTimer.Change(250, 250);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.Logger.LogDebug("1:Positioning Stop requested");

            this.shutterCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
            this.ParentStateMachine.ChangeState(
                new PositioningTableStopState(
                    this.ParentStateMachine,
                    this.InverterStatus as IPositioningInverterStatus,
                    this.Logger));
        }

        /// <inheritdoc />
        public override bool ValidateCommandMessage(InverterMessage message)
        {
            this.Logger.LogTrace($"2:message={message}:Is Error={message.IsError}");

            return true;
        }

        /// <inheritdoc />
        public override bool ValidateCommandResponse(InverterMessage message)
        {
            var returnValue = false;

            if (message.IsError)
            {
                this.Logger.LogError($"1:message={message}");
                this.ParentStateMachine.ChangeState(new PositioningTableErrorState(this.ParentStateMachine, this.InverterStatus, this.Logger));
            }
            else
            {
                this.Logger.LogTrace($"2:message={message}:Parameter Id={message.ParameterId}");
            }
            return returnValue;
        }

        private void ShutterCheck(object state)
        {
            bool ok = true;
            //if (this.data.ShutterPosition != ShutterPosition.None && this.inverterShutter != null)
            //{
            //    ok = (this.inverterShutter.CurrentShutterPosition == this.data.ShutterPosition);
            //}
            //if (ok)
            //{
            //    this.shutterCheckTimer.Change(Timeout.Infinite, Timeout.Infinite);
            //    this.ParentStateMachine.ChangeState(new PositioningTableEnableOperationState(this.ParentStateMachine, this.data, this.InverterStatus as IPositioningInverterStatus, this.Logger));
            //}
            //else
            //{
            //    this.Logger.LogTrace("1:Shutter not ready for positioning");
            //}
        }

        #endregion
    }
}
