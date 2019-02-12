using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSCradleViewModel : BindableBase, IViewModel, ISSCradleViewModel
    {
        #region Fields

        private bool luPresentiInMachineSide;

        private bool luPresentInOperatorSide = true;

        private bool zeroPawlSensor;

        #endregion

        #region Properties

        public bool LuPresentiInMachineSide { get => this.luPresentiInMachineSide; set => this.SetProperty(ref this.luPresentiInMachineSide, value); }

        public bool LuPresentInOperatorSide { get => this.luPresentInOperatorSide; set => this.SetProperty(ref this.luPresentInOperatorSide, value); }

        public bool ZeroPawlSensor { get => this.zeroPawlSensor; set => this.SetProperty(ref this.zeroPawlSensor, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
