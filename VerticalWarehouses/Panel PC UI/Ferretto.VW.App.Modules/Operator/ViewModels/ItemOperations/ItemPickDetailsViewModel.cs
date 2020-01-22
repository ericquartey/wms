using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemPickDetailsViewModel : BaseItemOperationViewModel
    {
        #region Constructors

        public ItemPickDetailsViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IItemsWmsWebService itemsWmsWebService,
            IMissionsWmsWebService missionsWmsWebService,
            IMissionOperationsService missionOperationsService,
            IBayManager bayManager,
            IDialogService dialogService)
            : base(wmsImagesProvider, missionsWmsWebService, itemsWmsWebService, bayManager, missionOperationsService, dialogService)
        {
        }

        #endregion
    }
}
