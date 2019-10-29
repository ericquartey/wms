using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Obsolete]
    internal sealed class LoadFirstDrawerViewModel : BaseMainViewModel
    {
        #region Constructors

        public LoadFirstDrawerViewModel()
            : base(Services.PresentationMode.Installer)
        {
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            this.IsBackNavigationAllowed = true;
        }

        #endregion
    }
}
