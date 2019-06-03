using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.ShutterPositioning
{
    public class ShutterPositioningStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly IInverterShutterPositioningFieldMessageData shutterPositionData;

        private readonly IInverterStatusBase inverterStatus;

        private bool disposed;

        #endregion

        #region Constructors

        public ShutterPositioningStateMachine(IInverterShutterPositioningFieldMessageData shutterPositionData, BlockingConcurrentQueue<InverterMessage> inverterCommandQueue, IInverterStatusBase inverterStatus,
              IEventAggregator eventAggregator, ILogger logger): base(logger)
        {
            this.Logger.LogDebug("1:Method Start");

            this.InverterCommandQueue = inverterCommandQueue;
            this.shutterPositionData = shutterPositionData;
            this.inverterStatus = inverterStatus;
            this.EventAggregator = eventAggregator;            
        }

        #endregion

        #region Destructors

        ~ShutterPositioningStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void Start()
        {
            this.Logger.LogDebug("1:Method Start");

            this.CurrentState = new ShutterPositioningStartState(this, this.inverterStatus, this.shutterPositionData, this.Logger);
            this.CurrentState?.Start();
        }

        /// <inheritdoc/>
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
