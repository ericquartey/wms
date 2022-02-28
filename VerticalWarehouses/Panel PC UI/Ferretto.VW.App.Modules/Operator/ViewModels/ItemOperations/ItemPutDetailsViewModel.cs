using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemPutDetailsViewModel : BaseItemOperationViewModel
    {
        #region Fields

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private string batch;

        private bool isCarrefour;

        private bool isPackingListCodeAvailable;

        private bool isPackingListDescriptionAvailable;

        private string itemCode;

        private string itemDescription;

        private string listCode;

        private string listDescription;

        private string listRow;

        private string materialStatus;

        private string packagingType;

        private string position;

        private string productionDate;

        private string requestedQuantity;

        private DelegateCommand suspendCommand;

        #endregion

        #region Constructors

        public ItemPutDetailsViewModel(
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IMachineItemsWebService itemsWebService,
            IMissionOperationsService missionOperationsService,
            IMachineConfigurationWebService machineConfigurationWebService,
            IBayManager bayManager,
            IDialogService dialogService)
            : base(loadingUnitsWebService, itemsWebService, bayManager, missionOperationsService, dialogService)
        {
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
        }

        #endregion

        #region Properties

        public string Batch { get => this.batch; set => this.SetProperty(ref this.batch, value); }

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsCarrefour
        {
            get => this.isCarrefour;
            set => this.SetProperty(ref this.isCarrefour, value, this.RaiseCanExecuteChanged);
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

        public string ItemCode { get => this.itemCode; set => this.SetProperty(ref this.itemCode, value); }

        public string ItemDescription { get => this.itemDescription; set => this.SetProperty(ref this.itemDescription, value); }

        public DrawerActivityItemDetail ItemDetail { get; set; }

        public string ListCode { get => this.listCode; set => this.SetProperty(ref this.listCode, value); }

        public string ListDescription { get => this.listDescription; set => this.SetProperty(ref this.listDescription, value); }

        public string ListRow { get => this.listRow; set => this.SetProperty(ref this.listRow, value); }

        public string MaterialStatus { get => this.materialStatus; set => this.SetProperty(ref this.materialStatus, value); }

        public string PackagingType { get => this.packagingType; set => this.SetProperty(ref this.packagingType, value); }

        public string Position { get => this.position; set => this.SetProperty(ref this.position, value); }

        public string ProductionDate { get => this.productionDate; set => this.SetProperty(ref this.productionDate, value); }

        public string RequestedQuantity { get => this.requestedQuantity; set => this.SetProperty(ref this.requestedQuantity, value); }

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

            this.IsBackNavigationAllowed = true;

            if (this.Data is DrawerActivityItemDetail itemDetail)
            {
                this.ItemDetail = itemDetail;
            }

            var configuration = await this.machineConfigurationWebService.GetAsync();
            this.IsCarrefour = configuration.Machine.IsCarrefour;
            this.Batch = this.ItemDetail.Batch;
            this.ItemCode = this.ItemDetail.ItemCode;
            this.ItemDescription = this.ItemDetail.ItemDescription;
            this.ListCode = this.ItemDetail.ListCode;
            this.ListDescription = this.ItemDetail.ListDescription;
            this.ListRow = this.ItemDetail.ListRow;
            this.MaterialStatus = this.ItemDetail.MaterialStatus;
            this.PackagingType = this.ItemDetail.PackageType;
            this.Position = this.ItemDetail.Position;
            this.ProductionDate = this.ItemDetail.ProductionDate;
            this.RequestedQuantity = this.ItemDetail.RequestedQuantity;

            this.IsPackingListCodeAvailable = !string.IsNullOrEmpty(this.MissionOperation?.PackingListCode);
            this.IsPackingListDescriptionAvailable = !string.IsNullOrEmpty(this.MissionOperation?.PackingListDescription);
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
