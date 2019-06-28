using Ferretto.VW.MAS_IODriver.StateMachines.PowerUp;

namespace Ferretto.VW.MAS_IODriver
{
    public partial class IoDevice
    {
        #region Methods

        public void ExecuteIoPowerUp()
        {
            this.currentStateMachine = new PowerUpStateMachine(this.ioCommandQueue, this.ioSHDStatus, this.index, this.eventAggregator, this.logger);
            this.currentStateMachine.Start();
        }

        #endregion
    }
}
