using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class LoadingUnitViewModel : BaseItemOperationViewModel
    {
        #region Constructors

        public LoadingUnitViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IMissionsDataService missionsDataService,
            IMissionOperationsService missionOperationsService,
            IBayManager bayManager)
            : base(wmsImagesProvider, missionsDataService, bayManager, missionOperationsService)
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
