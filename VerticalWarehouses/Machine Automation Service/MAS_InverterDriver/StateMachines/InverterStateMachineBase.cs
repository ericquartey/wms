using Ferretto.VW.Common_Utils.Utilities;

namespace Ferretto.VW.InverterDriver.StateMachines
{
    public abstract class InverterStateMachineBase : IInverterStateMachine
    {
        #region Fields

        protected BlockingConcurrentQueue<InverterMessage> inverterCommandQueue;

        protected BlockingConcurrentQueue<InverterMessage> priorityInverterCommandQueue;

        #endregion

        #region Properties

        protected IInverterState CurrentState { get; set; }

        #endregion

        #region Methods

        public virtual void ChangeState(IInverterState newState)
        {
            this.CurrentState = newState;
        }

        public void EnqueueMessage(InverterMessage message)
        {
            this.inverterCommandQueue.Enqueue(message);
        }

        public void NotifyMessage(InverterMessage message)
        {
            this.CurrentState?.NotifyMessage(message);
        }

        public abstract void Start();

        #endregion
    }
}
