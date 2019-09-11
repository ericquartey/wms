using Ferretto.VW.App.Controls;
using Prism.Regions;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class LoadFirstDrawerViewModel : BaseMainViewModel
    {
        #region Constructors

        public LoadFirstDrawerViewModel() : base(Services.PresentationMode.Installer)
        {
        }

        #endregion

        #region Methods

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            this.IsBackNavigationAllowed = true;
        }

        #endregion
    }
}
