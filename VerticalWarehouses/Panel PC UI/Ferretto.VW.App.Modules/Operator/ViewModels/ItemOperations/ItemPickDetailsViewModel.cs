using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemPickDetailsViewModel : BaseItemOperationViewModel
    {
        #region Constructors

        public ItemPickDetailsViewModel(
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
