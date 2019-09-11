using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Prism.Mvvm;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class CellsSideControlViewModel : BaseMainViewModel
    {
        #region Constructors

        public CellsSideControlViewModel() : base(Services.PresentationMode.Installer)
        {
        }

        #endregion

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

        public override Task OnNavigatedAsync()
        {
            return base.OnNavigatedAsync();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            this.IsBackNavigationAllowed = true;
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
