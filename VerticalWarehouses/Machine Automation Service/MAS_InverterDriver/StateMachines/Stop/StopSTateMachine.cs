using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.Stop
{
    public class StopStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly Axis axisToStop;

        private readonly ILogger logger;

        private bool disposed;

        #endregion

        #region Constructors

        public StopStateMachine(Axis axisToStop, BlockingConcurrentQueue<InverterMessage> inverterCommandQueue, IEventAggregator eventAggregator, ILogger logger)
        {
            logger.LogDebug("1:Method Start");

            this.axisToStop = axisToStop;
            this.InverterCommandQueue = inverterCommandQueue;
            this.EventAggregator = eventAggregator;
            this.logger = logger;
        }

        #endregion

        #region Destructors

        ~StopStateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.CurrentState = new StopState(this, this.axisToStop, this.logger);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.CurrentState.Stop();
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
