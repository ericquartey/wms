using System;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.ShutterPositioning
{
    public class ShutterPositioningStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly ShutterPosition shutterPosition;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningStateMachine(ShutterPosition shutterPosition, BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IEventAggregator eventAggregator, ILogger logger): base(logger)
        {
            this.Logger.LogDebug("1:Method Start");

            this.shutterPosition = shutterPosition;
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
            this.Logger.LogTrace($"2:Shutter Positioning={this.shutterPosition}");

            this.CurrentState = new ShutterPositioningStartState(this, this.shutterPosition, this.Logger);
            this.CurrentState?.Start();

            this.Logger.LogDebug("3:Method End");
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        #endregion
    }
}
