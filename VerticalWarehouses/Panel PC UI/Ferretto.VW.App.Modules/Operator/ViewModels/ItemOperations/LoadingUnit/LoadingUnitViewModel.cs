using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class LoadingUnitViewModel : BaseLoadingUnitViewModel
    {
        #region Fields

        public double inputQuantity;

        private readonly ICompartmentsWmsWebService compartmentsWmsWebService;

        private readonly IEventAggregator eventAggregator;

        private readonly IItemsWmsWebService itemsWmsWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMissionOperationsService missionOperationsService;

        private readonly IWmsDataProvider wmsDataProvider;

        private bool canInputQuantity;

        private DelegateCommand confirmOperationCommand;

        private int? currentLoadingUnitId;

        private string inputQuantityInfo;

        private bool isAdjustmentVisible;

        private bool isNewOperationAvailable;

        private bool isOperationVisible;

        private bool isPickVisible;

        private bool isPutVisible;

        private string measureUnit;

        private SubscriptionToken missionOperationToken;

        private DelegateCommand<string> operationCommand;

        private double quantityIncrement;

        private int quantityTolerance;

        private DelegateCommand recallLoadingUnitCommand;

        #endregion

        #region Constructors

        public LoadingUnitViewModel(
            IBayManager bayManager,
            IItemsWmsWebService itemsWmsWebService,
            ICompartmentsWmsWebService compartmentsWmsWebService,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMissionOperationsService missionOperationsService,
            ILoadingUnitsWmsWebService loadingUnitsWmsWebService,
            IEventAggregator eventAggregator,
            IWmsDataProvider wmsDataProvider)
            : base(bayManager, machineLoadingUnitsWebService, loadingUnitsWmsWebService)
        {
            this.itemsWmsWebService = itemsWmsWebService ?? throw new ArgumentNullException(nameof(itemsWmsWebService));
            this.compartmentsWmsWebService = compartmentsWmsWebService ?? throw new ArgumentNullException(nameof(compartmentsWmsWebService));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.missionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
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

        public string ConfirmOperationInfo => this.isNewOperationAvailable ? OperatorApp.ConfirmAndNewOperationsAvailable : OperatorApp.Confirm;

        public double InputQuantity
        {
            get => this.inputQuantity;
            set => this.SetProperty(ref this.inputQuantity, value);
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

        public string RecallLoadingUnitInfo => this.isNewOperationAvailable ? OperatorApp.NewOperationsAvailable : OperatorApp.RecallDrawer;

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.missionOperationToken != null)
            {
                this.EventAggregator.GetEvent<PubSubEvent<AssignedMissionOperationChangedEventArgs>>().Unsubscribe(this.missionOperationToken);
                this.missionOperationToken?.Dispose();
                this.missionOperationToken = null;
            }
        }

        public async override Task OnAppearedAsync()
        {
            this.missionOperationToken = this.eventAggregator.GetEvent<PubSubEvent<AssignedMissionOperationChangedEventArgs>>()
                                             .Subscribe(this.MissionOperationUpdate);

            var loadingUnitId = await this.GetLoadingUnitId();

            if (this.CanResetOperations(loadingUnitId))
            {
                this.ResetOperations(loadingUnitId);
            }

            await base.OnAppearedAsync();
        }

        public override void RaisePropertyChanged()
        {
            base.RaisePropertyChanged();

            this.RaisePropertyChanged(nameof(this.RecallLoadingUnitInfo));
            this.RaisePropertyChanged(nameof(this.ConfirmOperationInfo));
        }

        public async Task RecallLoadingUnitAsync()
        {
            try
            {
                this.IsBusyConfirmingRecallOperation = true;
                this.isWaitingForResponse = true;
                await this.machineLoadingUnitsWebService.RemoveFromBayAsync(this.LoadingUnit.Id);

                this.NavigationService.GoBack();
            }
            catch (MasWebApiException ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusyConfirmingRecallOperation = false;
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
            return !this.isWaitingForResponse
                   &&
                   !this.IsBusyConfirmingRecallOperation
                   &&
                   !this.IsBusyConfirmingOperation;
        }

        private bool CanDoOperation(string param)
        {
            return !(this.SelectedItem is null)
                   &&
                   !this.isWaitingForResponse
                   &&
                   !this.IsBusyConfirmingRecallOperation
                   &&
                   !this.IsBusyConfirmingOperation;
        }

        private bool CanRecallLoadingUnit()
        {
            return
                !this.isWaitingForResponse
                &&
                !this.IsBusyConfirmingOperation
                &&
                !this.IsBusyConfirmingRecallOperation
                &&
                !this.IsWaitingForResponse
                &&
                !(this.LoadingUnit is null);
        }

        private bool CanResetOperations(int? newLoadingUnitId)
        {
            if (this.currentLoadingUnitId != newLoadingUnitId
                &&
                !this.isNewOperationAvailable
                &&
                !this.IsBusyConfirmingOperation
                &&
                !this.IsBusyConfirmingRecallOperation)
            {
                return true;
            }

            return false;
        }

        private async Task ConfirmOperationAsync()
        {
            try
            {
                this.IsBusyConfirmingOperation = true;

                if (this.IsPickVisible)
                {
                    this.isWaitingForResponse = true;
                    await this.wmsDataProvider.PickAsync(this.SelectedItem.Id,
                                                         this.InputQuantity);
                }
                else if (this.IsPutVisible)
                {
                    this.isWaitingForResponse = true;
                    await this.wmsDataProvider.PutAsync(this.SelectedItem.Id,
                                                        this.InputQuantity);
                }
                else if (this.IsAdjustmentVisible)
                {
                    var compartment = new CompartmentDetails()
                    {
                        ItemId = this.SelectedItemCompartment.ItemId,
                        Stock = this.InputQuantity,
                    };

                    var newItemCompartment = await this.compartmentsWmsWebService.UpdateAsync(compartment, this.SelectedItemCompartment.Id);
                    this.SelectedItemCompartment.Stock = newItemCompartment.Stock;
                }
            }
            catch (Exception ex)
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
            var item = await this.itemsWmsWebService.GetByIdAsync(this.SelectedItemCompartment.ItemId.Value);
            this.QuantityTolerance = item.PickTolerance ?? 0;
            this.MeasureUnit = item.MeasureUnitDescription;
        }

        private async Task<int?> GetLoadingUnitId()
        {
            var bay = await this.BayManager.GetBayAsync();
            var loadingUnit = bay.Positions.Where(p => (p.LoadingUnit is null)).OrderByDescending(p => p.Height).Select(p => p.LoadingUnit).FirstOrDefault();
            return loadingUnit?.Id;
        }

        private void MissionOperationUpdate(AssignedMissionOperationChangedEventArgs e)
        {
            if (e.MissionOperationId.HasValue)
            {
                this.isNewOperationAvailable = true;
            }
        }

        private async Task ResetOperations(int? loadingUnitId)
        {
            this.isNewOperationAvailable = false;
            this.IsOperationVisible = false;
            this.IsPickVisible = false;
            this.IsPutVisible = false;
            this.IsAdjustmentVisible = false;
            this.isWaitingForResponse = false;

            this.currentLoadingUnitId = loadingUnitId;
        }

        private async Task SetTypeOperationAsync(string param)
        {
            this.ResetOperations(this.currentLoadingUnitId);
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

        #endregion
    }
}
