using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class LoadingUnitViewModel : BaseLoadingUnitViewModel, IOperationReasonsSelector
    {
        #region Fields

        private readonly IMachineCompartmentsWebService compartmentsWebService;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IMachineMissionsWebService machineMissionsWebService;

        private readonly IMachineService machineService;

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private readonly INavigationService navigationService;

        private readonly IOperatorNavigationService operatorNavigationService;

        private DelegateCommand cancelReasonCommand;

        private bool canInputQuantity;

        private DelegateCommand confirmOperationCommand;

        private DelegateCommand confirmReasonCommand;

        private double? inputQuantity;

        private string inputQuantityInfo;

        private bool isAdjustmentVisible;

        private bool isOperationVisible;

        private bool isPickVisible;

        private bool isPutVisible;

        private string measureUnit;

        private DelegateCommand<string> operationCommand;

        private double quantityIncrement;

        private int? quantityTolerance;

        private int? reasonId;

        private string reasonNotes;

        private IEnumerable<OperationReason> reasons;

        private DelegateCommand recallLoadingUnitCommand;

        private double? unitHeight;

        private int? unitNumber;

        private double? unitWeight;

        #endregion

        #region Constructors

        public LoadingUnitViewModel(
            INavigationService navigationService,
            IMachineItemsWebService itemsWebService,
            IMachineMissionsWebService machineMissionsWebService,
            IMachineCompartmentsWebService compartmentsWebService,
            IMachineService machineService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMissionOperationsService missionOperationsService,
            IOperatorNavigationService operatorNavigationService,
            IMachineMissionOperationsWebService missionOperationsWebService,
            IEventAggregator eventAggregator,
            IWmsDataProvider wmsDataProvider)
            : base(machineLoadingUnitsWebService, missionOperationsService, eventAggregator, wmsDataProvider)
        {
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.machineService = machineService ?? throw new ArgumentNullException(nameof(machineService));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.machineMissionsWebService = machineMissionsWebService ?? throw new ArgumentNullException(nameof(machineMissionsWebService));
            this.compartmentsWebService = compartmentsWebService ?? throw new ArgumentNullException(nameof(compartmentsWebService));
            this.operatorNavigationService = operatorNavigationService ?? throw new ArgumentNullException(nameof(operatorNavigationService));
            this.missionOperationsWebService = missionOperationsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWebService));
        }

        #endregion

        #region Properties

        public ICommand CancelReasonCommand =>
      this.cancelReasonCommand
      ??
      (this.cancelReasonCommand = new DelegateCommand(
          this.CancelReason));

        public bool CanInputQuantity
        {
            get => this.canInputQuantity;
            set => this.SetProperty(ref this.canInputQuantity, value);
        }

        public ICommand ConfirmOperationCommand =>
            this.confirmOperationCommand
            ??
            (this.confirmOperationCommand = new DelegateCommand(
                async () => await this.ConfirmOperationAsync(), this.CanConfirmOperation));

        public ICommand ConfirmReasonCommand =>
                          this.confirmReasonCommand
          ??
          (this.confirmReasonCommand = new DelegateCommand(
              async () => await this.ExecuteOperationAsync(),
              this.CanExecuteItemPick));

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
            set
            {
                if (this.SetProperty(ref this.isAdjustmentVisible, value) && value)
                {
                    this.IsPickVisible = false;
                    this.IsPutVisible = false;
                }
            }
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
            set
            {
                if (this.SetProperty(ref this.isPickVisible, value) && value)
                {
                    this.IsPutVisible = false;
                    this.IsAdjustmentVisible = false;
                }
            }
        }

        public bool IsPutVisible
        {
            get => this.isPutVisible;
            set
            {
                if (this.SetProperty(ref this.isPutVisible, value) && value)
                {
                    this.IsPickVisible = false;
                    this.IsAdjustmentVisible = false;
                }
            }
        }

        public string MeasureUnit
        {
            get => this.measureUnit;
            set => this.SetProperty(ref this.measureUnit, value);
        }

        public ICommand OperationCommand =>
            this.operationCommand
            ??
            (this.operationCommand = new DelegateCommand<string>(
                async (param) => await this.ToggleOperation(param), this.CanDoOperation));

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

        public int? ReasonId
        {
            get => this.reasonId;
            set => this.SetProperty(ref this.reasonId, value, this.RaiseCanExecuteChanged);
        }

        public string ReasonNotes
        {
            get => this.reasonNotes;
            set => this.SetProperty(ref this.reasonNotes, value);
        }

        public IEnumerable<OperationReason> Reasons
        {
            get => this.reasons;
            set => this.SetProperty(ref this.reasons, value);
        }

        public ICommand RecallLoadingUnitCommand =>
            this.recallLoadingUnitCommand
            ??
            (this.recallLoadingUnitCommand = new DelegateCommand(
                async () => await this.RecallLoadingUnitAsync(),
                this.CanRecallLoadingUnit));

        public double? UnitHeight
        {
            get => this.unitHeight;
            set => this.SetProperty(ref this.unitHeight, value, this.RaiseCanExecuteChanged);
        }

        public int? UnitNumber
        {
            get => this.unitNumber;
            set => this.SetProperty(ref this.unitNumber, value, this.RaiseCanExecuteChanged);
        }

        public double? UnitWeight
        {
            get => this.unitWeight;
            set => this.SetProperty(ref this.unitWeight, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public async Task<bool> CheckReasonsAsync()
        {
            this.ReasonId = null;

            try
            {
                this.IsBusyConfirmingOperation = true;

                var missionOperationType = MissionOperationType.NotSpecified;
                if (this.IsPickVisible)
                {
                    missionOperationType = MissionOperationType.Pick;
                }
                else if (this.IsPutVisible)
                {
                    missionOperationType = MissionOperationType.Put;
                }
                else if (this.IsAdjustmentVisible)
                {
                    missionOperationType = MissionOperationType.Inventory;
                }

                if (missionOperationType != MissionOperationType.NotSpecified)
                {
                    this.ReasonNotes = null;
                    this.Reasons = await this.missionOperationsWebService.GetAllReasonsAsync(missionOperationType);
                }

                if (this.reasons?.Any() == true)
                {
                    if (this.reasons.Count() == 1)
                    {
                        this.ReasonId = this.reasons.First().Id;
                    }
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.Reasons = null;
            }
            finally
            {
                this.IsBusyConfirmingOperation = false;
            }

            return this.Reasons?.Any() == true;
        }

        public async Task GetLoadingUnitsAsync()
        {
            try
            {
                var count = 0;

                var moveUnitId = await this.machineMissionsWebService.GetAllUnitGoBayAsync();

                if (moveUnitId != null)
                {
                    foreach (var unit in moveUnitId)
                    {
                        count++;
                    }
                }

                var moveUnitIdToCell = await this.machineMissionsWebService.GetAllUnitGoCellAsync();

                if (moveUnitIdToCell != null)
                {
                    var userdifference = moveUnitIdToCell.Except(moveUnitId);

                    if (userdifference.Any())
                    {
                        foreach (var units in userdifference)
                        {
                            var selectedunit = this.machineService.Loadunits.Where(i => i.Id == units).SingleOrDefault();
                            this.unitNumber = selectedunit.Id;
                            this.unitHeight = selectedunit.Height;
                            this.unitWeight = selectedunit.NetWeight;
                        }
                    }
                    else
                    {
                        this.unitNumber = this.LoadingUnit.Id;
                        this.unitHeight = this.LoadingUnit.Height;
                        this.unitWeight = this.LoadingUnit.NetWeight;
                    }
                }
                else
                {
                    this.unitNumber = this.LoadingUnit.Id;
                    this.unitHeight = this.LoadingUnit.Height;
                    this.unitWeight = this.LoadingUnit.NetWeight;
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                this.RaisePropertyChanged(nameof(this.unitNumber));
                this.RaisePropertyChanged(nameof(this.unitHeight));
                this.RaisePropertyChanged(nameof(this.unitWeight));
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            if (!this.CheckUDC())
            {
                return;
            }

            this.Reasons = null;

            Task.Run(async () =>
            {
                do
                {
                        await Task.Delay(500);
                        await this.GetLoadingUnitsAsync();
                    }
                while (this.IsVisible);
            });
        }

        public override void RaisePropertyChanged()
        {
            base.RaisePropertyChanged();

            this.RaisePropertyChanged(nameof(this.ConfirmOperationInfo));
            this.RaisePropertyChanged(nameof(this.IsItemStockVisible));
        }

        public async Task RecallLoadingUnitAsync()
        {
            try
            {
                this.IsBusyConfirmingRecallOperation = true;
                this.IsWaitingForResponse = true;

                var activeOperation = this.MissionOperationsService.ActiveWmsOperation;
                this.Logger.Debug($"User requested recall of loading unit.");

                if (activeOperation != null)
                {
                    var quantity = this.ItemsCompartments.FirstOrDefault(ic => ic.Id == activeOperation.CompartmentId && ic.ItemId == activeOperation.ItemId)?.Stock ?? activeOperation.RequestedQuantity;

                    var canComplete = await this.MissionOperationsService.CompleteAsync(activeOperation.Id, quantity);
                    if (!canComplete)
                    {
                        this.Logger.Debug($"Operation '{activeOperation.Id}' cannot be completed, forcing recall of loading unit.");

                        await this.MissionOperationsService.RecallLoadingUnitAsync(this.LoadingUnit.Id);
                    }
                }
                else
                {
                    await this.MissionOperationsService.RecallLoadingUnitAsync(this.LoadingUnit.Id);
                }

                this.navigationService.GoBackTo(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.ItemOperations.WAIT);

                this.Reset();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
                this.IsBusyConfirmingRecallOperation = false;
            }
        }

        protected async override Task OnMachineModeChangedAsync(MAS.AutomationService.Contracts.Hubs.MachineModeChangedEventArgs e)
        {
            await base.OnMachineModeChangedAsync(e);

            this.RaiseCanExecuteChanged();
        }

        protected override void OnSelectedCompartmentChanged()
        {
            this.HideOperation();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.operationCommand?.RaiseCanExecuteChanged();
            this.confirmOperationCommand?.RaiseCanExecuteChanged();
            this.recallLoadingUnitCommand?.RaiseCanExecuteChanged();
            this.confirmReasonCommand?.RaiseCanExecuteChanged();
        }

        private void CancelReason()
        {
            this.Reasons = null;
        }

        private bool CanConfirmOperation()
        {
            return
                this.IsWmsEnabledAndHealthy
                &&
                !this.IsWaitingForResponse
                &&
                this.InputQuantity.HasValue
                &&
                !this.IsBusyConfirmingRecallOperation
                &&
                !this.IsBusyConfirmingOperation;
        }

        private bool CanDoOperation(string param)
        {
            return
                this.SelectedItemCompartment?.ItemId != null
                &&
                !this.IsWaitingForResponse
                &&
                !this.IsBusyConfirmingRecallOperation
                &&
                !this.IsBusyConfirmingOperation
                &&
                this.IsWmsHealthy;
        }

        private bool CanExecuteItemPick()
        {
            return !(this.reasonId is null);
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

        private bool CheckUDC()
        {
            try
            {
                var activeOperation = this.MissionOperationsService.ActiveWmsOperation;

                // if (activeOperation != null && activeOperation.CompartmentId != null && activeOperation.CompartmentId > 0)
                if (activeOperation != null && activeOperation.CompartmentId > 0 && activeOperation.ItemId > 0)
                {
                    this.SelectedItemCompartment = this.ItemsCompartments.Where(s => s.Id == activeOperation.CompartmentId && s.ItemId == activeOperation.ItemId).FirstOrDefault();
                    this.RaisePropertyChanged(nameof(this.SelectedItemCompartment));
                }
                else if (!this.MachineService.Loadunits.Any(l => l.Id == this.LoadingUnit?.Id && l.Status == LoadingUnitStatus.InBay))
                {
                    this.navigationService.GoBackTo(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.WAIT);
                    return false;
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            return true;
        }

        private async Task ConfirmOperationAsync()
        {
            this.IsWaitingForResponse = true;

            var waitForReason = await this.CheckReasonsAsync();

            if (!waitForReason)
            {
                await this.ExecuteOperationAsync();
            }
        }

        private async Task ExecuteOperationAsync()
        {
            try
            {
                this.IsBusyConfirmingOperation = true;

                if (this.IsPickVisible)
                {
                    this.IsWaitingForResponse = true;

                    await this.WmsDataProvider.PickAsync(
                        this.SelectedItem.ItemId.Value,
                        this.InputQuantity.Value,
                        this.reasonId,
                        this.reasonNotes);
                }
                else if (this.IsPutVisible)
                {
                    this.IsWaitingForResponse = true;

                    await this.WmsDataProvider.PutAsync(
                        this.SelectedItem.ItemId.Value,
                        this.InputQuantity.Value,
                        this.reasonId,
                        this.reasonNotes);
                }
                else if (this.IsAdjustmentVisible)
                {
                    await this.compartmentsWebService.UpdateItemStockAsync(
                        this.SelectedItemCompartment.Id,
                        this.SelectedItemCompartment.ItemId.Value,
                        this.InputQuantity.Value,
                        this.reasonId,
                        this.reasonNotes);

                    await this.OnDataRefreshAsync();
                    this.IsBusyConfirmingOperation = false;
                }
                else
                {
                    this.IsBusyConfirmingOperation = false;
                }

                this.HideOperation();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.IsBusyConfirmingOperation = false;
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
                this.Reasons = null;
                this.RaiseCanExecuteChanged();
            }
        }

        private async Task GetItemInfoAsync()
        {
            if (this.SelectedItemCompartment?.ItemId is null)
            {
                return;
            }

            try
            {
                this.IsWaitingForResponse = true;
                var item = await this.itemsWebService.GetByIdAsync(this.SelectedItemCompartment.ItemId.Value);

                this.QuantityTolerance = item.PickTolerance ?? 0;
                this.MeasureUnit = item.MeasureUnitDescription;
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                throw;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void HideOperation()
        {
            this.IsAdjustmentVisible = false;
            this.IsPickVisible = false;
            this.IsPutVisible = false;

            this.IsOperationVisible = false;
        }

        private async Task ToggleOperation(string operationType)
        {
            if (!this.SelectedItemCompartment.ItemId.HasValue)
            {
                return;
            }

            var previousIsListModeEnabled = this.IsListModeEnabled;
            this.IsListModeEnabled = false;

            try
            {
                await this.GetItemInfoAsync();

                if (operationType == OperatorApp.Pick)
                {
                    this.InputQuantity = 0;
                    this.IsPickVisible = !this.IsPickVisible;
                    this.InputQuantityInfo = string.Format(Localized.Get("OperatorApp.PickingQuantity"), this.MeasureUnit);
                }
                else if (operationType == OperatorApp.Put)
                {
                    this.InputQuantity = 0;
                    this.IsPutVisible = !this.IsPutVisible;
                    this.InputQuantityInfo = string.Format(Localized.Get("OperatorApp.PutQuantity"), this.MeasureUnit);
                }
                else if (operationType == OperatorApp.Adjustment)
                {
                    this.InputQuantity = this.SelectedItemCompartment.Stock;
                    this.IsAdjustmentVisible = !this.IsAdjustmentVisible;
                    this.InputQuantityInfo = string.Format(Localized.Get("OperatorApp.AdjustmentQuantity"), this.MeasureUnit);
                }
                else
                {
                    this.ShowNotification(string.Format(Localized.Get("OperatorApp.InvalidOperation"), operationType));
                    return;
                }

                this.IsOperationVisible =
                    this.IsPickVisible
                    ||
                    this.IsPutVisible
                    ||
                    this.IsAdjustmentVisible;

                this.CanInputQuantity = this.IsOperationVisible;

                this.RaisePropertyChanged();
                this.RaiseCanExecuteChanged();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.IsListModeEnabled = previousIsListModeEnabled;
            }
        }

        #endregion
    }
}
