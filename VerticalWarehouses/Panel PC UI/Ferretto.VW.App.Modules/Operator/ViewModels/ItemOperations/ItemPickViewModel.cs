using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemPickViewModel : BaseItemOperationMainViewModel
    {
        #region Constructors

        public ItemPickViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IMissionsDataService missionsDataService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IBayManager bayManager)
            : base(wmsImagesProvider, missionsDataService, bayManager, eventAggregator, missionOperationsService)
        {
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.InputQuantity = this.MissionOperation.RequestedQuantity;
        }

        protected override void ShowOperationDetails()
        {
            this.NavigationService.Appear(
               nameof(Utils.Modules.Operator),
               Utils.Modules.Operator.ItemOperations.PICK_DETAILS,
               null,
               trackCurrentView: true);
        }

        #endregion
    }
}
