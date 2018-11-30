using Prism.Mvvm;
using System.Collections.Generic;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SensorsState
{
    internal class SSBaysViewModel : BindableBase
    {
        #region Fields

        private bool luPresentInBay;

        #endregion Fields

        #region Properties

        public System.Boolean LuPresentInBay { get => this.luPresentInBay; set => this.SetProperty(ref this.luPresentInBay, value); }

        #endregion Properties
    }
}
