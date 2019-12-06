using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemPutViewModel : BaseItemOperationMainViewModel
    {
        #region Constructors

        public ItemPutViewModel(
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

        public async override Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.InputQuantity = this.MissionOperation.RequestedQuantity;
        }

        protected override void ShowOperationDetails()
        {
            this.NavigationService.Appear(
               nameof(Utils.Modules.Operator),
               Utils.Modules.Operator.ItemOperations.PUT_DETAILS,
               null,
               trackCurrentView: true);
        }

        #endregion
    }
}
