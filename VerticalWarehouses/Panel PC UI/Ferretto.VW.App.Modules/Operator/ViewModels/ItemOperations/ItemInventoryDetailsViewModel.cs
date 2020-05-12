using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemInventoryDetailsViewModel : BaseItemOperationViewModel
    {
        #region Constructors

        public ItemInventoryDetailsViewModel(
            IMachineItemsWebService itemsWebService,
            IMissionOperationsService missionOperationsService,
            IBayManager bayManager,
            IDialogService dialogService)
            : base(itemsWebService, bayManager, missionOperationsService, dialogService)
        {
        }

        #endregion
    }
}
