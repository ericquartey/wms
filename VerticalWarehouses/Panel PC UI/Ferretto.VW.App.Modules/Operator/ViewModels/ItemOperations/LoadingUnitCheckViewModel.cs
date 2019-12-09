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
            IMissionsDataService missionsDataService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IBayManager bayManager)
            : base(wmsImagesProvider, missionsDataService, bayManager, eventAggregator, missionOperationsService)
        {
        }

        #endregion

        #region Methods

        protected override void ShowOperationDetails()
        {
        }

        #endregion
    }
}
