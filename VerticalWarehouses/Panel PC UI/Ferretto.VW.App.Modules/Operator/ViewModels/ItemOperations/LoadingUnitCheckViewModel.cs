using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class LoadingUnitCheckViewModel : BaseItemOperationMainViewModel
    {
        #region Constructors

        public LoadingUnitCheckViewModel(
            IMachineItemsWebService itemsWebService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            IDialogService dialogService)
            : base(itemsWebService, bayManager, eventAggregator, missionOperationsService, dialogService)
        {
        }

        #endregion

        #region Methods

        protected override void ShowOperationDetails()
        {
            // do nothing
        }

        #endregion
    }
}
