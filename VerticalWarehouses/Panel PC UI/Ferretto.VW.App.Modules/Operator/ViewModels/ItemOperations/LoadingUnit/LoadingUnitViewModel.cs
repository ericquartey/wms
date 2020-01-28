using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class LoadingUnitViewModel : BaseLoadingUnitViewModel
    {
        #region Fields

        public double inputQuantity;

        private readonly IBayManager bayManager;

        private readonly IItemsWmsWebService itemsWmsWebService;

        private readonly ILoadingUnitsWmsWebService loadingUnitsWmsWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private bool canInputQuantity;

        private DelegateCommand confirmOperationCommand;

        private string inputQuantityInfo;

        private bool isAdjustmentVisible;

        private bool isOperationVisible;

        private bool isPickVisible;

        private bool isPutVisible;

        private string measureUnit;

        private DelegateCommand<string> operationCommand;

        private double quantityIncrement;

        private int quantityTolerance;

        private DelegateCommand recallLoadingUnitCommand;

        #endregion

        #region Constructors

        public LoadingUnitViewModel(
               IBayManager bayManager,
               IItemsWmsWebService itemsWmsWebService,
               IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
               ILoadingUnitsWmsWebService loadingUnitsWmsWebService)
               : base(bayManager, machineLoadingUnitsWebService, loadingUnitsWmsWebService)
        {
            this.itemsWmsWebService = itemsWmsWebService ?? throw new ArgumentNullException(nameof(itemsWmsWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService;
        }

        #endregion

        #region Properties

        public bool CanInputQuantity
        {
            get => this.canInputQuantity;
            set => this.SetProperty(ref this.canInputQuantity, value, this.RaiseCanExecuteChanged);
        }

        public ICommand ConfirmOperationCommand =>
               this.confirmOperationCommand
                ??
                (this.confirmOperationCommand = new DelegateCommand(async () => await this.ConfirmOperationAsync(), this.CanConfirmOperation));

        public double InputQuantity
        {
            get => this.inputQuantity;
            set => this.SetProperty(ref this.inputQuantity, value);
        }

        public string InputQuantityInfo
        {
            get => this.inputQuantityInfo;
            set => this.SetProperty(ref this.inputQuantityInfo, value, this.RaiseCanExecuteChanged);
        }

        public bool IsAdjustmentVisible
        {
            get => this.isAdjustmentVisible;
            set => this.SetProperty(ref this.isAdjustmentVisible, value);
        }

        public bool IsOperationVisible
        {
            get => this.isOperationVisible;
            set => this.SetProperty(ref this.isOperationVisible, value, this.RaiseCanExecuteChanged);
        }

        public bool IsPickVisible
        {
            get => this.isPickVisible;
            set => this.SetProperty(ref this.isPickVisible, value);
        }

        public bool IsPutVisible
        {
            get => this.isPutVisible;
            set => this.SetProperty(ref this.isPutVisible, value);
        }

        public string MeasureUnit
        {
            get => this.measureUnit;
            set => this.SetProperty(ref this.measureUnit, value);
        }

        public ICommand OperationCommand =>
               this.operationCommand
                ??
                (this.operationCommand = new DelegateCommand<string>(async (param) => await this.SetTypeOperationAsync(param), this.CanDoOperation));

        public double QuantityIncrement
        {
            get => this.quantityIncrement;
            set => this.SetProperty(ref this.quantityIncrement, value);
        }

        public int QuantityTolerance
        {
            get => this.quantityTolerance;
            set
            {
                if (this.SetProperty(ref this.quantityTolerance, value))
                {
                    this.QuantityIncrement = Math.Pow(10, -this.quantityTolerance);
                }
            }
        }

        public ICommand RecallLoadingUnitCommand =>
                            this.recallLoadingUnitCommand
            ??
            (this.recallLoadingUnitCommand = new DelegateCommand(
                async () => await this.RecallLoadingUnitAsync(),
                this.CanRecallLoadingUnit));

        #endregion

        #region Methods

        public virtual bool CanRecallLoadingUnit()
        {
            return
                !this.IsWaitingForResponse
                &&
                this.LoadingUnit != null
                &&
                this.MachineModeService.MachineMode is MachineMode.Automatic;
        }

        public async override Task OnAppearedAsync()
        {
            this.ResetOperations();

            await base.OnAppearedAsync();
        }

        public override void RaisePropertyChanged()
        {
            base.RaisePropertyChanged();
        }

        public async Task RecallLoadingUnitAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.machineLoadingUnitsWebService.RemoveFromBayAsync(this.LoadingUnit.Id);

                this.NavigationService.GoBack();
            }
            catch (MasWebApiException ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected async override Task OnMachineModeChangedAsync(MAS.AutomationService.Contracts.Hubs.MachineModeChangedEventArgs e)
        {
            await base.OnMachineModeChangedAsync(e);

            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.operationCommand.RaiseCanExecuteChanged();
            this.confirmOperationCommand.RaiseCanExecuteChanged();
            this.recallLoadingUnitCommand.RaiseCanExecuteChanged();
        }

        private bool CanConfirmOperation()
        {
            return this.MachineModeService.MachineMode is MachineMode.Automatic;
        }

        private bool CanDoOperation(string param)
        {
            return !(this.SelectedItem is null);
        }

        private Task ConfirmOperationAsync()
        {
            // TO DO confirm operation
            return Task.CompletedTask;
        }

        private async Task GetItemInfoAsync()
        {
            var item = await this.itemsWmsWebService.GetByIdAsync(this.SelectedItemCompartment.ItemId.Value);
            this.QuantityTolerance = item.PickTolerance ?? 0;
            this.MeasureUnit = item.MeasureUnitDescription;
        }

        private async Task SetTypeOperationAsync(string param)
        {
            this.ResetOperations();
            this.IsOperationVisible = true;
            this.IsListVisibile = false;

            if (param == OperatorApp.Pick)
            {
                this.IsPickVisible = true;
                this.InputQuantityInfo = string.Format(OperatorApp.PickingQuantity, this.MeasureUnit);
            }
            else if (param == OperatorApp.Put)
            {
                this.IsPutVisible = true;
                this.InputQuantityInfo = string.Format(OperatorApp.PutQuantity, this.MeasureUnit);
            }
            else if (param == OperatorApp.Adjustment)
            {
                this.IsAdjustmentVisible = true;
                this.InputQuantityInfo = string.Format(OperatorApp.AdjustmentQuantity, this.MeasureUnit);
            }

            await this.GetItemInfoAsync();

            this.CanInputQuantity = true;
            this.InputQuantity = this.SelectedItemCompartment.Stock;

            this.RaisePropertyChanged();
            this.RaiseCanExecuteChanged();
        }


        private void ResetOperations()
        {
            this.IsOperationVisible = false;
            this.IsPickVisible = false;
            this.IsPutVisible = false;
            this.IsAdjustmentVisible = false;
        }

        #endregion
    }
}
