using System.Threading.Tasks;
using Ferretto.VW.InstallationApp.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class LoadingDrawersViewModel : BindableBase, ILoadingDrawersViewModel
    {
        #region Properties

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task OnEnterViewAsync()
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
