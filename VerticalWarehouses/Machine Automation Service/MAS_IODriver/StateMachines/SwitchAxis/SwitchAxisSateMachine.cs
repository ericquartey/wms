using System.Threading;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Utilities;
using Prism.Events;

namespace Ferretto.VW.MAS_IODriver.StateMachines.SwitchAxis
{
    public class SwitchAxisSateMachine : IoStateMachineBase
    {
        #region Fields

        private const int PauseInterval = 250;

        private readonly Axis axisToSwitchOn;

        private Timer delayTimer;

        private bool disposed;

        private bool switchOffOtherAxis;

        #endregion

        #region Constructors

        public SwitchAxisSateMachine(Axis axisToSwitchOn, bool switchOffOtherAxis, BlockingConcurrentQueue<IoMessage> ioCommandQueue, IEventAggregator eventAggregator)
        {
            this.axisToSwitchOn = axisToSwitchOn;
            this.switchOffOtherAxis = switchOffOtherAxis;
            this.ioCommandQueue = ioCommandQueue;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Destructors

        ~SwitchAxisSateMachine()
        {
            this.Dispose(false);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessMessage(IoMessage message)
        {
            if (message.ValidOutputs && !message.ElevatorMotorOn && !message.CradleMotorOn)
            {
                this.delayTimer = new Timer(this.DelayElapsed, null, PauseInterval, -1);    //VALUE -1 period means timer does not fire multiple times
            }
            base.ProcessMessage(message);
        }

        public override void Start()
        {
            if (this.switchOffOtherAxis)
            {
                this.CurrentState = new SwitchOffMotorState(axisToSwitchOn, this);
            }
            else
            {
                this.CurrentState = new SwitchOnMotorState(this.axisToSwitchOn, this);
            }
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
                this.CurrentState.Dispose();
            }

            this.disposed = true;

            base.Dispose(disposing);
        }

        private void DelayElapsed(object state)
        {
            ChangeState(new SwitchOnMotorState(this.axisToSwitchOn, this));
        }

        #endregion
    }
}
