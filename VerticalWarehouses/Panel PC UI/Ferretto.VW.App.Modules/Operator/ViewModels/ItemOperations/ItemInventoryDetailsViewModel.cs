using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemInventoryDetailsViewModel : BaseItemOperationViewModel
    {
        #region Constructors

        public ItemInventoryDetailsViewModel(
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IMachineItemsWebService itemsWebService,
            IMissionOperationsService missionOperationsService,
            IBayManager bayManager,
            IDialogService dialogService)
            : base(loadingUnitsWebService, itemsWebService, bayManager, missionOperationsService, dialogService)
        {
        }

        #endregion
    }
}
