using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class AddingItemToLoadingUnitViewModel : BaseOperatorViewModel, IOperationalContextViewModel
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly IDialogService dialogService;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private readonly INavigationService navigationService;

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private bool acquiredLotValue;

        private bool acquiredSerialNumberValue;

        private DelegateCommand addItemCommand;

        private DelegateCommand cancelReasonCommand;

        private int compartmentId;

        private DelegateCommand confirmReasonCommand;

        private string expireDate;

        private bool expireDateVisibility;

        private double inputQuantity;

        private bool isAddItemButtonEnabled;

        private bool isFromList;

        private bool isOrderVisible;

        private bool isReasonVisible;

        private string itemDescription;

        private int itemId;

        private int loadingUnitId;

        private string lot;

        private bool isLotFilter;

        private bool lotVisibility;

        private string measureUnitTxt;

        private MissionOperation missionOperation;

        private int? orderId;

        private IEnumerable<OperationReason> orders;

        private double quantityIncrement;

        private int? quantityTolerance;

        private int? reasonId;

        private string reasonNotes;

        private IEnumerable<OperationReason> reasons;

        private string serialNumber;

        private bool serialNumberVisibility;

        private string titleText;

        #endregion

        #region Constructors

        public AddingItemToLoadingUnitViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineItemsWebService itemsWebService,
            IMachineMissionOperationsWebService missionOperationsWebService,
            INavigationService navigationService,
            IDialogService dialogService,
            IMachineConfigurationWebService machineConfigurationWebService,
            IAuthenticationService authenticationService)
           : base(PresentationMode.Operator)
        {
            this.Logger.Info("Ctor AddingItemToLoadingUnitViewModel");

            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.missionOperationsWebService = missionOperationsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWebService));
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
        }

        #endregion

        #region Properties

        public string ActiveContextName => !this.IsLotFilter ? OperationalContext.ItemInventory.ToString() : OperationalContext.AddItem.ToString();

        public ICommand AddItemCommand =>
                  this.addItemCommand
                  ??
                  (this.addItemCommand = new DelegateCommand(
                      async () => await this.AddItemToLoadingUnitAsync(),
                      this.CanAddItemButton));

        public ICommand CancelReasonCommand =>
           this.cancelReasonCommand
           ??
           (this.cancelReasonCommand = new DelegateCommand(
               this.CancelReason));

        public ICommand ConfirmReasonCommand =>
                          this.confirmReasonCommand
          ??
          (this.confirmReasonCommand = new DelegateCommand(
              async () => await this.ExecuteOperationAsync(),
              this.CanExecuteItemPick));

        public string ExpireDate
        {
            get => this.expireDate;
            set
            {
                if (this.SetProperty(ref this.expireDate, value))
                {
                    // this.TriggerSearchAsync().GetAwaiter(); // Do not perform the searching routine
                }
            }
        }

        public bool ExpireDateVisibility
        {
            get => this.expireDateVisibility;
            set => this.SetProperty(ref this.expireDateVisibility, value, this.RaiseCanExecuteChanged);
        }

        public bool IsLotFilter
        {
            get => this.isLotFilter;
            set => this.SetProperty(ref this.isLotFilter, value, this.RaiseCanExecuteChanged);
        }

        public double InputQuantity
        {
            get => this.inputQuantity;
            set => this.SetProperty(ref this.inputQuantity, value, () =>
            {
                this.IsAddItemButtonEnabled = value > 0;
            });
        }

        public bool IsAddItemButtonEnabled
        {
            get => this.isAddItemButtonEnabled;
            protected set => this.SetProperty(ref this.isAddItemButtonEnabled, value, this.RaiseCanExecuteChanged);
        }

        public bool IsFromList
        {
            get => this.isFromList;
            set => this.SetProperty(ref this.isFromList, value, this.RaiseCanExecuteChanged);
        }

        public bool IsOrderVisible
        {
            get => this.isOrderVisible;
            set => this.SetProperty(ref this.isOrderVisible, value);
        }

        public bool IsReasonVisible
        {
            get => this.isReasonVisible;
            set => this.SetProperty(ref this.isReasonVisible, value);
        }

        public bool IsWaitingForReason { get; private set; }

        public string ItemDescription
        {
            get => this.itemDescription;
            set => this.SetProperty(ref this.itemDescription, value, this.RaiseCanExecuteChanged);
        }

        public int LoadingUnitId
        {
            get => this.loadingUnitId;
            set => this.SetProperty(ref this.loadingUnitId, value, this.RaiseCanExecuteChanged);
        }

        public string Lot
        {
            get => this.lot;
            set
            {
                if (this.SetProperty(ref this.lot, value))
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        this.acquiredLotValue = false;
                    }
                }
            }
        }

        public bool LotVisibility
        {
            get => this.lotVisibility;
            set => this.SetProperty(ref this.lotVisibility, value, this.RaiseCanExecuteChanged);
        }

        public string MeasureUnitTxt
        {
            get => this.measureUnitTxt;
            set => this.SetProperty(ref this.measureUnitTxt, value, this.RaiseCanExecuteChanged);
        }

        public MissionOperation MissionOperation
        {
            get => this.missionOperation;
            set => this.SetProperty(ref this.missionOperation, value, this.RaiseCanExecuteChanged);
        }

        public int? OrderId
        {
            get => this.orderId;
            set => this.SetProperty(ref this.orderId, value, this.RaiseCanExecuteChanged);
        }

        public IEnumerable<OperationReason> Orders
        {
            get => this.orders;
            set => this.SetProperty(ref this.orders, value);
        }

        public double QuantityIncrement
        {
            get => this.quantityIncrement;
            set => this.SetProperty(ref this.quantityIncrement, value, this.RaiseCanExecuteChanged);
        }

        public int? QuantityTolerance
        {
            get => this.quantityTolerance;
            set
            {
                this.SetProperty(ref this.quantityTolerance, value);
                this.QuantityIncrement = Math.Pow(10, -this.quantityTolerance.Value);
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

        public string SerialNumber
        {
            get => this.serialNumber;
            set
            {
                if (this.SetProperty(ref this.serialNumber, value))
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        this.acquiredSerialNumberValue = false;
                    }
                }
            }
        }

        public bool SerialNumberVisibility
        {
            get => this.serialNumberVisibility;
            set => this.SetProperty(ref this.serialNumberVisibility, value, this.RaiseCanExecuteChanged);
        }

        public string TitleText
        {
            get => this.titleText;
            set => this.SetProperty(ref this.titleText, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public async Task<bool> CheckReasonsAsync()
        {
            this.ReasonId = null;
            this.OrderId = null;

            try
            {
                var missionOperationType = MissionOperationType.Put;

                this.ReasonNotes = null;
                this.Reasons = await this.missionOperationsWebService.GetAllReasonsAsync(missionOperationType);

                if (this.reasons?.Any() == true)
                {
                    if (this.reasons.Count() == 1)
                    {
                        this.ReasonId = this.reasons.First().Id;
                    }
                }

                this.IsOrderVisible = this.Reasons != null && this.Reasons.Any() && this.Orders != null && this.Orders.Any();
                this.IsReasonVisible = this.Reasons != null && this.Reasons.Any() && (this.Orders == null || !this.Orders.Any());
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.Reasons = null;
                this.Orders = null;
                this.IsOrderVisible = false;
                this.IsReasonVisible = false;
            }

            return this.Reasons?.Any() == true;
        }

        public async Task CommandUserActionAsync(UserActionEventArgs userAction)
        {
            if (userAction is null)
            {
                return;
            }

            if (userAction.IsReset)
            {
                return;
            }

            var readValue = userAction.Code;
            var readLot = true ? readValue : userAction.Parameters["ItemLot"];

            // Check and update: first Lot, then SerialNumber. Be careful about the order: do not
            // change it
            if (this.lotVisibility && this.serialNumberVisibility)
            {
                if (this.acquiredLotValue && this.acquiredSerialNumberValue)
                {
                    this.acquiredLotValue = false;
                    this.acquiredSerialNumberValue = false;

                    this.Lot = string.Empty;
                    this.SerialNumber = string.Empty;
                }

                if (this.acquiredLotValue && !this.acquiredSerialNumberValue)
                {
                    this.SerialNumber = readValue;
                    this.acquiredSerialNumberValue = true;
                }

                if (!this.acquiredLotValue)
                {
                    this.Lot = readLot;
                    this.acquiredLotValue = true;
                }
            }

            if (this.lotVisibility && !this.serialNumberVisibility)
            {
                this.Lot = readLot;
                this.acquiredLotValue = true;
                this.acquiredSerialNumberValue = false;
            }

            if (!this.lotVisibility && this.serialNumberVisibility)
            {
                this.SerialNumber = readValue;
                this.acquiredLotValue = false;
                this.acquiredSerialNumberValue = true;
            }

            if (!this.lotVisibility && !this.serialNumberVisibility)
            {
                this.acquiredLotValue = false;
                this.acquiredSerialNumberValue = false;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.acquiredLotValue = false;
            this.acquiredSerialNumberValue = false;

            var config = await this.machineConfigurationWebService.GetConfigAsync();
            this.IsLotFilter = config.LotFilter;

            this.InputQuantity = 1;
            this.QuantityTolerance = 0;
            this.Lot = null;
            this.SerialNumber = null;
            this.ExpireDate = null;
            this.MissionOperation = null;

            this.TitleText = Localized.Get("OperatorApp.AddingItemPageHeader");

            if (this.Data is ItemAddedToLoadingUnitDetail dataBundle)
            {
                if (dataBundle.MissionOperation != null)
                {
                    this.MissionOperation = dataBundle.MissionOperation;
                    this.InputQuantity = this.MissionOperation.RequestedQuantity;

                    this.Lot = this.MissionOperation.Lot;
                    this.SerialNumber = this.MissionOperation.SerialNumber;
                }

                this.IsFromList = this.MissionOperation != null;

                this.itemId = dataBundle.ItemId;
                this.compartmentId = dataBundle.CompartmentId;

                this.LoadingUnitId = dataBundle.LoadingUnitId;
                this.ItemDescription = dataBundle.ItemDescription;
                this.MeasureUnitTxt = dataBundle.MeasureUnitTxt;
                this.QuantityTolerance = dataBundle.QuantityTolerance ?? 0;

                this.LotVisibility = await this.itemsWebService.IsItemHandledByLotAsync(this.itemId);
                this.SerialNumberVisibility = await this.itemsWebService.IsItemHandledBySerialNumberAsync(this.itemId);
            }

            await base.OnAppearedAsync();
        }

        public override void Disappear()
        {
            base.Disappear();

            this.IsReasonVisible = false;
            this.IsOrderVisible = false;

        }

        protected override void RaiseCanExecuteChanged()
        {
            this.addItemCommand.RaiseCanExecuteChanged();
            this.cancelReasonCommand?.RaiseCanExecuteChanged();
            this.confirmReasonCommand?.RaiseCanExecuteChanged();
        }

        private async Task AddItemToLoadingUnitAsync()
        {
            this.IsWaitingForResponse = true;
            this.NoteEnabled = true;
            var waitForReason = await this.CheckReasonsAsync();

            this.IsWaitingForReason = waitForReason;

            if (!waitForReason) // Force true ImmediateAddItem dont handle ResonNote
            {
                await this.ExecuteOperationAsync();
            }

            this.IsWaitingForResponse = false;
        }

        private bool CanAddItemButton()
        {
            return this.isAddItemButtonEnabled;
        }

        private void CancelReason()
        {
            this.Reasons = null;
            this.Orders = null;
            this.IsOrderVisible = false;
            this.IsReasonVisible = false;
            this.IsWaitingForResponse = false;
            this.IsWaitingForReason = false;
        }

        private bool CanExecuteItemPick()
        {
            return !(this.reasonId is null) && !this.IsWaitingForResponse;
        }

        private async Task ExecuteOperationAsync()
        {
            this.ClearNotifications();
            this.IsWaitingForResponse = true;
            try
            {
                this.ShowNotification(Localized.Get("OperatorApp.ItemAdding"), Services.Models.NotificationSeverity.Info);

                if (this.MissionOperation != null)
                {
                    this.Logger.Debug($"Immediate adding item {this.itemId} by list {this.missionOperation.ItemListRowCode} into loading unit {this.LoadingUnitId} ...");
                    // No ReasonNote
                    await this.machineLoadingUnitsWebService.ImmediateAddItemByListAsync(
                                         this.LoadingUnitId,
                                         this.missionOperation.ItemListRowCode,
                                         this.missionOperation.ItemListCode,
                                         this.InputQuantity,
                                         this.compartmentId,
                                         this.authenticationService.UserName);
                }
                else
                {
                    this.Logger.Debug($"Immediate adding item {this.itemId} into loading unit {this.LoadingUnitId} ...");
                    // Add ReasonNote
                    await this.machineLoadingUnitsWebService.AddItemReasonsAsync(
                                         this.LoadingUnitId,
                                         this.itemId,
                                         this.InputQuantity,
                                         this.compartmentId,
                                         this.Lot,
                                         this.SerialNumber,
                                         this.ReasonId,
                                         this.ReasonNotes);
                }

                this.ShowNotification(Localized.Get("OperatorApp.ItemLoaded"), Services.Models.NotificationSeverity.Success);

                this.NavigationService.GoBack();
            }
            catch (Exception exc)
            {
                this.Logger.Debug($"Immediate adding item {this.itemId} into loading unit {this.LoadingUnitId} failed. Error: {exc}");
                this.ShowNotification(Localized.Get("OperatorApp.ItemAddingFailed"), Services.Models.NotificationSeverity.Error);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
