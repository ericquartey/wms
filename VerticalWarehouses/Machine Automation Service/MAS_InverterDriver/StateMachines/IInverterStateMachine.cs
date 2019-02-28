namespace Ferretto.VW.MAS_InverterDriver.StateMachines
{
    public interface IInverterStateMachine
    {
        #region Methods

        void ChangeState(IInverterState newState);

        void EnqueueMessage(InverterMessage message);

        void NotifyMessage(InverterMessage message);

        void Start();

        #endregion
    }
}
