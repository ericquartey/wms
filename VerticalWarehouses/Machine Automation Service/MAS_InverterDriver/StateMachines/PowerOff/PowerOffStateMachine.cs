using Ferretto.VW.MAS_InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS_Utils.Utilities;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_InverterDriver.StateMachines.PowerOff
{
    public class PowerOffStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly IInverterStatusBase inverterStatus;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerOffStateMachine(IInverterStatusBase inverterStatus, BlockingConcurrentQueue<InverterMessage> inverterCommandQueue, IEventAggregator eventAggregator, ILogger logger)
            : base( logger )
        {
            this.Logger.LogDebug( "1:Method Start" );

            this.inverterStatus = inverterStatus;
            this.InverterCommandQueue = inverterCommandQueue;
            this.EventAggregator = eventAggregator;
        }

        #endregion

        #region Destructors

        ~PowerOffStateMachine()
        {
            this.Dispose( false );
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.CurrentState = new PowerOffStartState( this, this.inverterStatus, this.Logger );
            CurrentState?.Start();
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

            base.Dispose( disposing );
        }

        #endregion
    }
}
