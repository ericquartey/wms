using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Positioning
{
    public class PositioningStateMachine : InverterStateMachineBase
    {
        #region Fields

        private const int SEND_DELAY = 50;

        private IPositioningFieldMessageData data;

        private bool disposed;

        #endregion

        #region Constructors

        public PositioningStateMachine(IPositioningFieldMessageData data, BlockingConcurrentQueue<InverterMessage> inverterCommandQueue, IEventAggregator eventAggregator, ILogger logger)
        {
            this.Logger = logger;
            this.Logger.LogDebug("1:Method Start");

            this.data = data;
            this.InverterCommandQueue = inverterCommandQueue;
            this.EventAggregator = eventAggregator;

            this.Logger.LogDebug("2:Method End");
        }

        #endregion

        #region Destructors

        ~PositioningStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug("1:Method Start");

            this.InverterCommandQueue.Enqueue(new InverterMessage(0x00, (short)InverterParameterId.PositionTargetPositionParam, this.data.TargetPosition, SEND_DELAY));
            this.InverterCommandQueue.Enqueue(new InverterMessage(0x00, (short)InverterParameterId.PositionTargetSpeedParam, this.data.TargetSpeed, SEND_DELAY));
            this.InverterCommandQueue.Enqueue(new InverterMessage(0x00, (short)InverterParameterId.PositionAccelerationParam, this.data.TargetAcceleration, SEND_DELAY));
            this.InverterCommandQueue.Enqueue(new InverterMessage(0x00, (short)InverterParameterId.PositionDecelerationParam, this.data.TargetDeceleration, SEND_DELAY));

            this.CurrentState = new VoltageDisabledState(this, this.data, this.Logger);

            this.Logger.LogDebug("2:Method End");
        }

        public override void Stop()
        {
            this.Logger.LogDebug("1:Method Start");

            this.CurrentState.Stop();

            this.Logger.LogDebug("2:Method End");
        }

        #endregion
    }
}
