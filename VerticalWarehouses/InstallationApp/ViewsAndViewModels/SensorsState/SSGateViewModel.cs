using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSGateViewModel : BindableBase, IViewModel, ISSGateViewModel
    {
        #region Fields

        private bool gateSensorA;

        private bool gateSensorB = true;

        #endregion

        #region Properties

        public bool GateSensorA { get => this.gateSensorA; set => this.SetProperty(ref this.gateSensorA, value); }

        public bool GateSensorB { get => this.gateSensorB; set => this.SetProperty(ref this.gateSensorB, value); }

        #endregion

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

        #endregion
    }
}
