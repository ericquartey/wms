using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemInventoryDetailsViewModel : BaseItemOperationViewModel
    {
        #region Constructors

        public ItemInventoryDetailsViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IMissionsWmsWebService missionsWmsWebService,
            IMissionOperationsService missionOperationsService,
            IBayManager bayManager,
            IDialogService dialogService)
            : base(wmsImagesProvider, missionsWmsWebService, bayManager, missionOperationsService, dialogService)
        {
        }

        #endregion
    }
}
