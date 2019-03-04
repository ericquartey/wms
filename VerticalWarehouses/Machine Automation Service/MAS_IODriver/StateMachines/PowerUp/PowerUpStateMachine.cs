using System;
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

        #endregion

        #region Constructors

        public PowerUpStateMachine(BlockingConcurrentQueue<IoMessage> ioCommandQueue, IEventAggregator eventAggregator)
        {
            this.ioCommandQueue = ioCommandQueue;
            this.eventAggregator = eventAggregator;
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
            this.CurrentState = new ClearOutputs(this);
        }

        private void DelayElapsed(object state)
        {
            var pulseIoMessage = new IoMessage(false);

            EnqueueMessage(pulseIoMessage);
        }

        #endregion
    }
}
