using Ferretto.VW.MAS_IODriver.StateMachines.Reset;

namespace Ferretto.VW.MAS_IODriver
{
    public partial class IoDevice
    {
        #region Methods

        public void ExecuteIoReset()
        {
            this.currentStateMachine = new ResetStateMachine(this.ioCommandQueue, this.ioSHDStatus, this.eventAggregator, this.logger);
            this.currentStateMachine.Start();
        }

        #endregion
    }
}
