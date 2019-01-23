using Prism.Mvvm;
using System.Collections.Generic;

namespace Ferretto.VW.InstallationApp
{
    public class SSBaysViewModel : BindableBase, IViewModel, ISSBaysViewModel
    {
        #region Fields

        private bool luPresentInBay;

        #endregion Fields

        #region Properties

        public System.Boolean LuPresentInBay { get => this.luPresentInBay; set => this.SetProperty(ref this.luPresentInBay, value); }

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
