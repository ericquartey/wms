using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.Compartment
{
    public class LayoutViewModel : BaseNavigationViewModel
    {
        #region Methods

        protected override void OnAppear()
        {
            base.OnAppear();
            var navigationService = ServiceLocator.Current.GetInstance<INavigationService>();
            navigationService.Appear(nameof(Ferretto.Common.Utils.Modules.Compartment), Ferretto.Common.Utils.Modules.Compartment.COMPARTMENT);
        }

        #endregion Methods
    }
}
