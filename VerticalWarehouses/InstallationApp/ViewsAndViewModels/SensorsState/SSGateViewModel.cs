using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SensorsState
{
    internal class SSGateViewModel : BindableBase
    {
        #region Fields

        private bool gateSensorA;
        private bool gateSensorB = true;

        #endregion Fields

        #region Properties

        public System.Boolean GateSensorA { get => this.gateSensorA; set => this.SetProperty(ref this.gateSensorA, value); }
        public System.Boolean GateSensorB { get => this.gateSensorB; set => this.SetProperty(ref this.gateSensorB, value); }

        #endregion Properties
    }
}
