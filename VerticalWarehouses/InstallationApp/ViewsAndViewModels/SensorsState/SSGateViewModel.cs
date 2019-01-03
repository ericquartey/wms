using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSGateViewModel : BindableBase, IViewModel, ISSGateViewModel
    {
        #region Fields

        private bool gateSensorA;
        private bool gateSensorB = true;

        #endregion Fields

        #region Properties

        public System.Boolean GateSensorA { get => this.gateSensorA; set => this.SetProperty(ref this.gateSensorA, value); }

        public System.Boolean GateSensorB { get => this.gateSensorB; set => this.SetProperty(ref this.gateSensorB, value); }

        #endregion Properties

        #region Methods

        public void ExitFromViewMethod()
        {
            throw new System.NotImplementedException();
        }

        public void SubscribeMethodToEvent()
        {
            throw new System.NotImplementedException();
        }

        public void UnSubscribeMethodFromEvent()
        {
            throw new System.NotImplementedException();
        }

        #endregion Methods
    }
}
