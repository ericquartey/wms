using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSBaysViewModel : BindableBase, IViewModel, ISSBaysViewModel
    {
        #region Fields

        private bool luPresentInBay;

        #endregion

        #region Properties

        public bool LuPresentInBay { get => this.luPresentInBay; set => this.SetProperty(ref this.luPresentInBay, value); }

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
