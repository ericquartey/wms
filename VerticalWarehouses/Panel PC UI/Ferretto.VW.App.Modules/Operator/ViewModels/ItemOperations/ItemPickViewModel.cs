using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemPickViewModel : BaseItemOperationMainViewModel
    {
        #region Constructors

        public ItemPickViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IMissionsDataService missionsDataService,
            IMissionOperationsService missionOperationsService,
            IBayManager bayManager)
            : base(wmsImagesProvider, missionsDataService, bayManager, missionOperationsService)
        {
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

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
               Utils.Modules.Operator.ItemOperations.PICK_DETAILS,
               null,
               trackCurrentView: true);
        }

        #endregion
    }
}
