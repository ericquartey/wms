using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [System.Obsolete]
    internal sealed class DrawerLoadingUnloadingTestViewModel : BaseMainViewModel
    {
        #region Fields

        private ICommand exitFromViewCommand;

        #endregion

        #region Constructors

        public DrawerLoadingUnloadingTestViewModel()
            : base(Services.PresentationMode.Installer)
        {
        }

        #endregion

        #region Properties

        public ICommand ExitFromViewCommand => this.exitFromViewCommand ?? (this.exitFromViewCommand = new DelegateCommand(this.ExitFromViewMethod));

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
        }

        public Task OnEnterViewAsync()
        {
            return Task.CompletedTask;
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
