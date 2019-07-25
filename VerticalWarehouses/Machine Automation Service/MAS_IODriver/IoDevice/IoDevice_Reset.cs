using Ferretto.VW.MAS.IODriver.StateMachines.Reset;

namespace Ferretto.VW.MAS.IODriver.IoDevice
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
