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
