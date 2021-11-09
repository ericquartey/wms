using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemPickDetailsViewModel : BaseItemOperationViewModel
    {
        #region Fields

        private bool isCurrentDraperyItem;

        private bool isPackingListCodeAvailable;

        private bool isPackingListDescriptionAvailable;

        private DelegateCommand suspendCommand;

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

        public bool IsPackingListCodeAvailable
        {
            get => this.isPackingListCodeAvailable;
            set => this.SetProperty(ref this.isPackingListCodeAvailable, value, this.RaiseCanExecuteChanged);
        }

        public bool IsPackingListDescriptionAvailable
        {
            get => this.isPackingListDescriptionAvailable;
            set => this.SetProperty(ref this.isPackingListDescriptionAvailable, value, this.RaiseCanExecuteChanged);
        }

        public ICommand SuspendCommand =>
                  this.suspendCommand
                  ??
                  (this.suspendCommand = new DelegateCommand(
                      async () => await this.SuspendOperationAsync(),
                      this.CanSuspendButton));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsPackingListCodeAvailable = !string.IsNullOrEmpty(this.MissionOperation.PackingListCode);
            this.IsPackingListDescriptionAvailable = !string.IsNullOrEmpty(this.MissionOperation.PackingListDescription);

            var item = await this.ItemsWebService.GetByIdAsync(this.MissionOperation.ItemId);
            this.IsCurrentDraperyItem = item.IsDraperyItem;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
        }

        private bool CanSuspendButton()
        {
            return true;
        }

        #endregion
    }
}
