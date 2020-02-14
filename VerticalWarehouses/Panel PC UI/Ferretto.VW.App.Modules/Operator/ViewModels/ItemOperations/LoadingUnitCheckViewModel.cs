using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class LoadingUnitCheckViewModel : BaseItemOperationMainViewModel
    {
        #region Constructors

        public LoadingUnitCheckViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IMachineItemsWebService itemsWebService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            IDialogService dialogService)
            : base(wmsImagesProvider, itemsWebService, bayManager, eventAggregator, missionOperationsService, dialogService)
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
