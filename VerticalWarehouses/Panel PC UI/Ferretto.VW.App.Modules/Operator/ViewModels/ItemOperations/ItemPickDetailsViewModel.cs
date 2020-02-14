using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemPickDetailsViewModel : BaseItemOperationViewModel
    {
        #region Constructors

        public ItemPickDetailsViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IMachineItemsWebService itemsWebService,
            IMissionOperationsService missionOperationsService,
            IBayManager bayManager,
            IDialogService dialogService)
            : base(wmsImagesProvider, itemsWebService, bayManager, missionOperationsService, dialogService)
        {
        }

        #endregion
    }
}
