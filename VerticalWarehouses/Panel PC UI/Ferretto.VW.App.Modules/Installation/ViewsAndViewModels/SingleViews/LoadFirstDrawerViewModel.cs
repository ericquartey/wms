using System.Threading.Tasks;
using Ferretto.VW.App.Installation.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews
{
    internal class LoadFirstDrawerViewModel : BindableBase, ILoadFirstDrawerViewModel
    {
        #region Properties

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public Task OnEnterViewAsync()
        {
            return Task.CompletedTask;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
