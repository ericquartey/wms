using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemPickDetailsViewModel : BaseItemOperationViewModel
    {
        #region Constructors

        public ItemPickDetailsViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IMissionsDataService missionsDataService,
            IMissionOperationsService missionOperationsService,
            IBayManager bayManager)
            : base(wmsImagesProvider, missionsDataService, bayManager, missionOperationsService)
        {
        }

        #endregion
    }
}
