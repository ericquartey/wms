using System;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.ShutterPositioning
{
    public class ShutterPositioningStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly IShutterPositioningFieldMessageData data;

        #endregion

        #region Constructors

        public ShutterPositioningStateMachine(IShutterPositioningFieldMessageData data, BlockingConcurrentQueue<InverterMessage> inverterCommandQueue, IEventAggregator eventAggregator, ILogger logger)
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

        ~ShutterPositioningStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        public override void Start()
        {
            this.Logger.LogDebug("1:Method Start");

            this.InverterCommandQueue.Enqueue(new InverterMessage(this.data.SystemIndex, (short)InverterParameterId.ShutterTargetPosition, this.data.ShutterPosition));

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
