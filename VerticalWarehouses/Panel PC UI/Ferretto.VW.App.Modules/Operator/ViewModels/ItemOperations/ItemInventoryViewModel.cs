using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemInventoryViewModel : BaseItemOperationMainViewModel
    {
        #region Constructors

        public ItemInventoryViewModel(
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
            this.NavigationService.Appear(
              nameof(Utils.Modules.Operator),
              Utils.Modules.Operator.ItemOperations.INVENTORY_DETAILS,
              null,
              trackCurrentView: true);
        }

        #endregion
    }
}
