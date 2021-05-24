using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemInventoryDetailsViewModel : BaseItemOperationViewModel
    {
        #region Fields

        private bool isPackingListCodeAvailable;

        private bool isPackingListDescriptionAvailable;

        #endregion

        #region Constructors

        public ItemInventoryDetailsViewModel(
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

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsPackingListCodeAvailable = !string.IsNullOrEmpty(this.MissionOperation.PackingListCode);
            this.IsPackingListDescriptionAvailable = !string.IsNullOrEmpty(this.MissionOperation.PackingListDescription);
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
