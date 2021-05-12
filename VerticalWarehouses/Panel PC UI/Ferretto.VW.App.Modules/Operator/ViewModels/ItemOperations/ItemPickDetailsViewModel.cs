using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemPickDetailsViewModel : BaseItemOperationViewModel
    {
        #region Fields

        private bool isCurrentDraperyItem;

        #endregion

        // Note:
        // Only For TendaggiParadiso
        // - The MissionOperation.ItemNotes contains the information about the "Destination"
        // - The MissionOperation.ItemListShipmentUnitCode contains the information about the "Shipment day"

        #region Constructors

        public ItemPickDetailsViewModel(
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IMachineItemsWebService itemsWebService,
            IMissionOperationsService missionOperationsService,
            IBayManager bayManager,
            IDialogService dialogService)
            : base(loadingUnitsWebService, itemsWebService, bayManager, missionOperationsService, dialogService)
        {
        }

        #endregion

        #region Properties

        public bool IsCurrentDraperyItem
        {
            get => this.isCurrentDraperyItem;
            set => this.SetProperty(ref this.isCurrentDraperyItem, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            var item = await this.ItemsWebService.GetByIdAsync(this.MissionOperation.ItemId);
            this.IsCurrentDraperyItem = item.IsDraperyItem;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
