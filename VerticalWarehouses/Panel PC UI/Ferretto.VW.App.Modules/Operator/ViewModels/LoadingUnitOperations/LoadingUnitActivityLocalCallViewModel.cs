using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class LoadingUnitActivityLocalCallViewModel : BaseLoadingUnitOperationViewModel
    {
        #region Constructors

        public LoadingUnitActivityLocalCallViewModel(
                    IWmsImagesProvider wmsImagesProvider,
                    IMissionsDataService missionsDataService,
                    IBayManager bayManager)
                    : base(wmsImagesProvider, missionsDataService, bayManager)
        {
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;
        }

        #endregion
    }
}
