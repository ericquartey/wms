using Ferretto.VW.App.Controls;
using Prism.Regions;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [System.Obsolete]
    internal sealed class SaveRestoreConfigViewModel : BaseMainViewModel
    {
        #region Constructors

        public SaveRestoreConfigViewModel()
            : base(Services.PresentationMode.Installer)
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
