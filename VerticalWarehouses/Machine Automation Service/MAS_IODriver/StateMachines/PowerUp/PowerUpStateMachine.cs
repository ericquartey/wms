using System.Threading;
using Ferretto.VW.Common_Utils.Utilities;
using Prism.Events;

namespace Ferretto.VW.MAS_IODriver.StateMachines.PowerUp
{
    public class PowerUpStateMachine : IoStateMachineBase
    {
        #region Fields

        private const int PulseInterval = 350;

        private Timer delayTimer;

        private bool disposed;

        #endregion

        #region Constructors

        public PowerUpStateMachine(BlockingConcurrentQueue<IoMessage> ioCommandQueue, IEventAggregator eventAggregator)
        {
            this.ioCommandQueue = ioCommandQueue;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Destructors

        ~PowerUpStateMachine()
        {
            Dispose(false);
        }

        #endregion

        #region Methods

        public override void ProcessMessage(IoMessage message)
        {
            if (message.ValidOutputs && message.ResetSecurity)
            {
                this.delayTimer = new Timer(DelayElapsed, null, PulseInterval, -1);    //VALUE -1 period means timer does not fire multiple times
            }
            base.ProcessMessage(message);
        }

        public override void Start()
        {
            this.CurrentState = new ClearOutputsState(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.delayTimer?.Dispose();
                CurrentState.Dispose();
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        private void DelayElapsed(object state)
        {
            var pulseIoMessage = new IoMessage(false);

            this.EnqueueMessage(pulseIoMessage);
        }

        #endregion
    }
}
