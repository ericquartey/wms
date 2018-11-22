using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SensorsState
{
    internal class SSCradleViewModel : BindableBase
    {
        #region Fields

        private bool luPresentiInMachineSide;
        private bool luPresentInOperatorSide = true;
        private bool zeroPawlSensor;

        #endregion Fields

        #region Properties

        public System.Boolean LuPresentiInMachineSide { get => this.luPresentiInMachineSide; set => this.SetProperty(ref this.luPresentiInMachineSide, value); }
        public System.Boolean LuPresentInOperatorSide { get => this.luPresentInOperatorSide; set => this.SetProperty(ref this.luPresentInOperatorSide, value); }
        public System.Boolean ZeroPawlSensor { get => this.zeroPawlSensor; set => this.SetProperty(ref this.zeroPawlSensor, value); }

        #endregion Properties
    }
}
