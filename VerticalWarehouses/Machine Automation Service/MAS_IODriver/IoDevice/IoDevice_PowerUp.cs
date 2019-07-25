using Ferretto.VW.MAS.IODriver.StateMachines.PowerUp;

namespace Ferretto.VW.MAS.IODriver.IoDevice
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
