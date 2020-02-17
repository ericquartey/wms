using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class LoadingUnitViewModel : BaseLoadingUnitViewModel
    {
        #region Fields

        public double? inputQuantity;

        private readonly IMachineCompartmentsWebService compartmentsWebService;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IMissionOperationsService missionOperationsService;

        private readonly IWmsDataProvider wmsDataProvider;

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

        private int? quantityTolerance;

        private DelegateCommand recallLoadingUnitCommand;

        #endregion

        #region Constructors

        public LoadingUnitViewModel(
            IMachineItemsWebService itemsWebService,
            IMachineCompartmentsWebService compartmentsWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IWmsDataProvider wmsDataProvider)
            : base(machineLoadingUnitsWebService, eventAggregator)
        {
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.compartmentsWebService = compartmentsWebService ?? throw new ArgumentNullException(nameof(compartmentsWebService));
            this.missionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.wmsDataProvider = wmsDataProvider ?? throw new ArgumentNullException(nameof(wmsDataProvider));
        }

        #endregion

        #region Properties

        public bool CanInputQuantity
        {
            get => this.canInputQuantity;
            set => this.SetProperty(ref this.canInputQuantity, value);
        }

        public ICommand ConfirmOperationCommand =>
               this.confirmOperationCommand
                ??
                (this.confirmOperationCommand = new DelegateCommand(async () => await this.ConfirmOperationAsync(), this.CanConfirmOperation));

        public double? InputQuantity
        {
            get => this.inputQuantity;
            set => this.SetProperty(ref this.inputQuantity, value, this.RaiseCanExecuteChanged);
        }

        public string InputQuantityInfo
        {
            get => this.inputQuantityInfo;
            set => this.SetProperty(ref this.inputQuantityInfo, value);
        }

        public bool IsAdjustmentVisible
        {
            get => this.isAdjustmentVisible;
            set => this.SetProperty(ref this.isAdjustmentVisible, value);
        }

        public bool IsItemStockVisible => this.isPickVisible || this.isPutVisible;

        public bool IsOperationVisible
        {
            get => this.isOperationVisible;
            set => this.SetProperty(ref this.isOperationVisible, value);
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

        public int? QuantityTolerance
        {
            get => this.quantityTolerance;
            set
            {
                if (this.SetProperty(ref this.quantityTolerance, value))
                {
                    this.QuantityIncrement = Math.Pow(10, -this.quantityTolerance.Value);
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

        public async override Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            if (this.CanReset)
            {
                this.ResetOperations();
            }
        }

        public override void RaisePropertyChanged()
        {
            base.RaisePropertyChanged();

            this.RaisePropertyChanged(nameof(this.RecallLoadingUnitInfo));
            this.RaisePropertyChanged(nameof(this.ConfirmOperationInfo));
            this.RaisePropertyChanged(nameof(this.IsItemStockVisible));
        }

        public async Task RecallLoadingUnitAsync()
        {
            try
            {
                this.IsBusyConfirmingRecallOperation = true;
                this.IsWaitingForNewOperation = true;
                await this.missionOperationsService.RecallLoadingUnitAsync(this.LoadingUnit.Id);

                this.NavigationService.GoBack();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusyConfirmingRecallOperation = false;
            }
        }

        public override void ResetOperations()
        {
            base.ResetOperations();

            this.IsOperationVisible = false;
            this.IsPickVisible = false;
            this.IsPutVisible = false;
            this.IsAdjustmentVisible = false;
        }

        protected async override Task OnMachineModeChangedAsync(MAS.AutomationService.Contracts.Hubs.MachineModeChangedEventArgs e)
        {
            await base.OnMachineModeChangedAsync(e);

            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.operationCommand?.RaiseCanExecuteChanged();
            this.confirmOperationCommand?.RaiseCanExecuteChanged();
            this.recallLoadingUnitCommand?.RaiseCanExecuteChanged();
        }

        private bool CanConfirmOperation()
        {
            return
                ConfigurationManager.AppSettings.GetWmsDataServiceEnabled()
                &&
                !this.IsWaitingForNewOperation
                &&
                this.InputQuantity.HasValue
                &&
                !this.IsBusyConfirmingRecallOperation
                &&
                !this.IsBusyConfirmingOperation;
        }

        private bool CanDoOperation(string param)
        {
            return !(this.SelectedItem is null)
                   &&
                   !this.IsWaitingForNewOperation
                   &&
                   !this.IsBusyConfirmingRecallOperation
                   &&
                   !this.IsBusyConfirmingOperation
                   &&
                   this.IsWmsHealthy;
        }

        private bool CanRecallLoadingUnit()
        {
            return
                !this.IsBusyConfirmingOperation
                &&
                !this.IsBusyConfirmingRecallOperation
                &&
                !(this.LoadingUnit is null);
        }

        private async Task ConfirmOperationAsync()
        {
            try
            {
                this.IsBusyConfirmingOperation = true;

                if (this.IsPickVisible)
                {
                    this.IsWaitingForNewOperation = true;

                    await this.wmsDataProvider.PickAsync(
                        this.SelectedItem.ItemId.Value,
                        this.InputQuantity.Value);
                }
                else if (this.IsPutVisible)
                {
                    this.IsWaitingForNewOperation = true;

                    await this.wmsDataProvider.PutAsync(
                        this.SelectedItem.ItemId.Value,
                        this.InputQuantity.Value);
                }
                else if (this.IsAdjustmentVisible)
                {
                    var compartment = new CompartmentDetails()
                    {
                        ItemId = this.SelectedItemCompartment.ItemId,
                        Stock = this.InputQuantity.Value,
                    };

                    var newItemCompartment = await this.compartmentsWebService.UpdateAsync(compartment, this.SelectedItemCompartment.Id);

                    await this.ResetLoadDataAsync();
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusyConfirmingOperation = false;
                this.RaiseCanExecuteChanged();
            }
        }

        private async Task GetItemInfoAsync()
        {
            var item = await this.itemsWebService.GetByIdAsync(this.SelectedItemCompartment.ItemId.Value);
            this.QuantityTolerance = item.PickTolerance ?? 0;
            this.MeasureUnit = item.MeasureUnitDescription;
        }

        private async Task SetTypeOperationAsync(string param)
        {
            this.ResetOperations();
            this.IsOperationVisible = true;
            this.IsListVisibile = false;

            await this.GetItemInfoAsync();

            if (param == OperatorApp.Pick)
            {
                this.InputQuantity = null;
                this.IsPickVisible = true;
                this.InputQuantityInfo = string.Format(OperatorApp.PickingQuantity, this.MeasureUnit);
            }
            else if (param == OperatorApp.Put)
            {
                this.InputQuantity = null;
                this.IsPutVisible = true;
                this.InputQuantityInfo = string.Format(OperatorApp.PutQuantity, this.MeasureUnit);
            }
            else if (param == OperatorApp.Adjustment)
            {
                this.InputQuantity = this.SelectedItemCompartment.Stock;
                this.IsAdjustmentVisible = true;
                this.InputQuantityInfo = string.Format(OperatorApp.AdjustmentQuantity, this.MeasureUnit);
            }

            this.CanInputQuantity = true;

            this.RaisePropertyChanged();
            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
