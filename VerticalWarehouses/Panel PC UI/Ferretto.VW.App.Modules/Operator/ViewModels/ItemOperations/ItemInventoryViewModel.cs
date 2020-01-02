using System.Threading.Tasks;
using Ferretto.VW.App.Accessories;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemInventoryViewModel : BaseItemOperationMainViewModel, IOperationalContextViewModel
    {
        #region Constructors

        public ItemInventoryViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IMissionsWmsWebService missionsWmsWebService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            IDialogService dialogService)
            : base(wmsImagesProvider, missionsWmsWebService, bayManager, eventAggregator, missionOperationsService, dialogService)
        {
        }

        #endregion

        #region Properties

        public string ActiveContextName => OperationalContext.ItemInventory.ToString();

        #endregion

        #region Methods

        public override bool CanConfirmOperation()
        {
            return
               !this.IsWaitingForResponse
               &&
               !this.IsBusyAbortingOperation
               &&
               !this.IsBusyConfirmingOperation
               &&
               this.InputQuantity.HasValue
               &&
               this.InputQuantity.Value >= 0;
        }

        public async Task CommandUserActionAsync(UserActionEventArgs userAction)
        {
        }

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
