using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class LoadingUnitCheckViewModel : BaseItemOperationMainViewModel
    {
        #region Constructors

        public LoadingUnitCheckViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IItemsWmsWebService itemsWmsWebService,
            IMissionsWmsWebService missionsWmsWebService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            IDialogService dialogService)
            : base(wmsImagesProvider, missionsWmsWebService, itemsWmsWebService, bayManager, eventAggregator, missionOperationsService, dialogService)
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
