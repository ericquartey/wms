using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Microsoft.AspNetCore.Http;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemPutViewModel : BaseItemOperationMainViewModel, IOperationalContextViewModel
    {
        #region Fields

        private readonly IBarcodeReaderService barcodeReaderService;

        private readonly IMachineItemsWebService itemsWebService;

        private DelegateCommand barcodeReaderCancelCommand;

        private DelegateCommand barcodeReaderConfirmCommand;

        private string barcodeString;

        private bool canPutBox;

        private bool confirmOperation;

        private DelegateCommand confirmOperationCommand;

        private bool confirmPartialOperation;

        private DelegateCommand confirmPartialOperationCommand;

        private DelegateCommand fullOperationCommand;

        private bool isBarcodeActive;

        private bool isCurrentDraperyItemFullyRequested;

        private bool isVisibleBarcodeReader;

        private DelegateCommand putBoxCommand;

        private DelegateCommand showBarcodeReaderCommand;

        #endregion

        #region Constructors

        public ItemPutViewModel(
            IBarcodeReaderService barcodeReaderService,
            ILaserPointerService deviceService,
            IMachineAreasWebService areasWebService,
            IMachineIdentityWebService machineIdentityWebService,
            IMachineConfigurationWebService machineConfigurationWebService,
            INavigationService navigationService,
            IOperatorNavigationService operatorNavigationService,
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IMachineCompartmentsWebService compartmentsWebService,
            IMachineMissionOperationsWebService missionOperationsWebService,
            IMachineItemsWebService itemsWebService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            IDialogService dialogService,
            IWmsDataProvider wmsDataProvider,
            IAuthenticationService authenticationService,
            IMachineAccessoriesWebService accessoriesWebService)
            : base(
                  deviceService,
                  areasWebService,
                  machineIdentityWebService,
                  machineConfigurationWebService,
                  navigationService,
                  operatorNavigationService,
                  loadingUnitsWebService,
                  itemsWebService,
                  compartmentsWebService,
                  missionOperationsWebService,
                  bayManager,
                  eventAggregator,
                  missionOperationsService,
                  dialogService,
                  wmsDataProvider,
                  authenticationService,
                  accessoriesWebService)
        {
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));

            this.barcodeReaderService = barcodeReaderService;
        }

        #endregion

        #region Properties

        public override string ActiveContextName => OperationalContext.ItemPut.ToString();

        public ICommand BarcodeReaderCancelCommand =>
                                    this.barcodeReaderCancelCommand
                    ??
                    (this.barcodeReaderCancelCommand = new DelegateCommand(
                        async () => this.BarcodeReaderCancel()));

        public ICommand BarcodeReaderConfirmCommand =>
                                            this.barcodeReaderConfirmCommand
                    ??
                    (this.barcodeReaderConfirmCommand = new DelegateCommand(
                        async () => this.BarcodeReaderConfirm()));

        public string BarcodeString
        {
            get => this.barcodeString;
            set => this.SetProperty(ref this.barcodeString, value, this.RaiseCanExecuteChanged);
        }

        public bool CanPutBox
        {
            get => this.canPutBox;
            set => this.SetProperty(ref this.canPutBox, value && this.IsBoxEnabled, this.RaiseCanExecuteChanged);
        }

        public bool ConfirmOperation
        {
            get => this.confirmOperation;
            set => this.SetProperty(ref this.confirmOperation, value);
        }

        public ICommand ConfirmOperationCommand =>
            this.confirmOperationCommand
            ??
            (this.confirmOperationCommand = new DelegateCommand(
                async () => await this.ConfirmOperationAsync(this.barcodeOk),
                this.CanConfirmOperationPut));

        public bool ConfirmPartialOperation
        {
            get => this.confirmPartialOperation;
            set => this.SetProperty(ref this.confirmPartialOperation, value);
        }

        public ICommand ConfirmPartialOperationCommand =>
            this.confirmPartialOperationCommand
            ??
            (this.confirmPartialOperationCommand = new DelegateCommand(
                async () => await this.ConfirmPartialOperationAsync(),
                this.CanConfirmPartialOperationPut));

        public ICommand FullOperationCommand =>
                    this.fullOperationCommand
            ??
            (this.fullOperationCommand = new DelegateCommand(
                async () => await this.ConfirmPartialOperationAsync(),
                this.CanPartiallyCompleteOnFullCompartment));

        public bool IsBarcodeActive
        {
            get => this.isBarcodeActive;
            set => this.SetProperty(ref this.isBarcodeActive, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCurrentDraperyItemFullyRequested
        {
            get => this.isCurrentDraperyItemFullyRequested;
            set => this.SetProperty(ref this.isCurrentDraperyItemFullyRequested, value, this.RaiseCanExecuteChanged);
        }

        public bool IsVisibleBarcodeReader
        {
            get => this.isVisibleBarcodeReader;
            set => this.SetProperty(ref this.isVisibleBarcodeReader, value, this.RaiseCanExecuteChanged);
        }

        public ICommand PutBoxCommand =>
                    this.putBoxCommand
            ??
            (this.putBoxCommand = new DelegateCommand(
                async () => await this.PutBoxAsync("0"),
                this.CanPutBoxes));

        public ICommand ShowBarcodeReaderCommand =>
            this.showBarcodeReaderCommand
            ??
            (this.showBarcodeReaderCommand = new DelegateCommand(this.ShowBarcodeReader));

        #endregion

        #region Methods

        public void BarcodeReaderCancel()
        {
            this.IsVisibleBarcodeReader = false;
            this.BarcodeString = string.Empty;
        }

        public void BarcodeReaderConfirm()
        {
            if (!string.IsNullOrEmpty(this.BarcodeString))
            {
                this.barcodeReaderService.SimulateRead(this.BarcodeString.EndsWith("\r") ? this.BarcodeString : this.BarcodeString + "\r");

                this.BarcodeString = string.Empty;
                this.IsVisibleBarcodeReader = false;
            }
        }

        public bool CanBarcodeReader()
        {
            return true;
        }

        public bool CanConfirmOperationPut()
        {
            try
            {
                this.confirmOperation = this.MissionOperation != null &&
                    this.InputQuantity.Value == this.MissionRequestedQuantity &&
                    !this.IsOperationCanceled;

                if (this.confirmOperation && this.IsDoubleConfirmBarcodePut && string.IsNullOrEmpty(this.barcodeOk))
                {
                    this.confirmOperation = false;
                }

                this.confirmPartialOperation = this.MissionOperation != null &&
                    this.InputQuantity.Value >= 0 &&
                    this.InputQuantity.Value != this.MissionRequestedQuantity &&
                    !this.IsOperationCanceled;

                if (this.confirmPartialOperation && this.IsDoubleConfirmBarcodePut && string.IsNullOrEmpty(this.barcodeOk))
                {
                    this.confirmPartialOperation = false;
                }

                this.RaisePropertyChanged(nameof(this.ConfirmOperation));

                this.RaisePropertyChanged(nameof(this.ConfirmPartialOperation));
            }
            catch (Exception)
            {
            }

            return
               !this.IsWaitingForResponse
               &&
               this.MissionOperation != null
               &&
               !this.IsBusyAbortingOperation
               &&
               !this.IsOperationConfirmed
               &&
               this.InputQuantity.HasValue
               &&
               this.InputQuantity.Value == this.MissionRequestedQuantity
               &&
               !this.CanPutBox;
        }

        public bool CanConfirmPartialOperationPut()
        {
            if (this.IsDoubleConfirmBarcodePut && string.IsNullOrEmpty(this.barcodeOk))
            {
                return false;
            }

            return
               !this.IsWaitingForResponse
               &&
               this.MissionOperation != null
               &&
               !this.IsBusyAbortingOperation
               &&
               !this.IsOperationConfirmed
               &&
               this.InputQuantity.HasValue
               &&
               this.InputQuantity.Value != this.MissionRequestedQuantity
               &&
               !this.canPutBox;
        }

        public async Task CommandUserActionAsync(UserActionEventArgs userAction)
        {
            if (userAction is null)
            {
                return;
            }

            if (this.CanPutBoxes() && userAction.UserAction == UserAction.VerifyItem)
            {
                await this.PutBoxAsync(userAction.Code);
                return;
            }

            if (userAction.UserAction == UserAction.VerifyItem && this.IsDoubleConfirmBarcodePut)
            {
                userAction.SetDoubleConfirm(true);
                await base.CommandUserActionAsync(userAction);
                this.CanConfirmOperationPut();
                this.CanPartiallyCompleteOnFullCompartment();
            }
            else
            {
                this.CanConfirmOperationPut();
                if (this.confirmOperation || this.confirmPartialOperation)
                {
                    if (userAction.UserAction == UserAction.VerifyItem)
                    {
                        userAction.SetDoubleConfirm(false);
                        var last = this.Mission.Operations.Last().Id == this.MissionOperation.Id;

                        if (last &&
                            this.BarcodeLenght > 0 &&
                            this.MissionOperation.SerialNumber == "1" &&
                            this.MissionOperation.RequestedQuantity - this.MissionOperation.DispatchedQuantity == 1.0)
                        {
                            var messageBoxResult = this.DialogService.ShowMessage(Localized.Get("OperatorApp.LastOperationMessage"), Localized.Get("OperatorApp.Warning"), DialogType.Exclamation, DialogButtons.OK);
                            if (messageBoxResult == DialogResult.OK)
                            {
                                await base.CommandUserActionAsync(userAction);
                            }
                        }
                        else if (!this.IsDoubleConfirmBarcodePut)
                        {
                            await base.CommandUserActionAsync(userAction);
                        }
                    }
                    else if (userAction.UserAction == UserAction.ConfirmKey)
                    {
                            await this.ConfirmOperationAsync(this.barcodeOk);

                    }
                }
            }
        }

        public async Task ConfirmOperationAsync(string barcode)
        {
            System.Diagnostics.Debug.Assert(
                this.InputQuantity.HasValue,
                "The input quantity should have a value");

            try
            {
                this.IsBusyConfirmingOperation = true;
                this.IsWaitingForResponse = true;
                this.ClearNotifications();

                this.IsOperationConfirmed = true;

                var item = await this.itemsWebService.GetByIdAsync(this.MissionOperation.ItemId);
                bool canComplete = false;
                var loadingUnitId = this.Mission.LoadingUnit.Id;
                var itemId = this.MissionOperation.ItemId;
                var type = this.MissionOperation.Type;
                var quantity = this.InputQuantity.Value;

                var isRequestConfirm = await this.MachineIdentityWebService.IsRequestConfirmForLastOperationOnLoadingUnitAsync();
                if (isRequestConfirm)
                {
                    var isLastMissionOnCurrentLoadingUnit = await this.MissionOperationsService.IsLastWmsMissionForCurrentLoadingUnitAsync(this.MissionOperation.Id);
                    if (isLastMissionOnCurrentLoadingUnit)
                    {
                        var messageBoxResult = this.DialogService.ShowMessage(
                            Localized.Get("InstallationApp.ConfirmationOperation"),
                            Localized.Get("InstallationApp.ConfirmationOperation"),
                            DialogType.Question,
                            DialogButtons.OK);
                        if (messageBoxResult is DialogResult.OK)
                        {
                            // go away...
                        }
                    }
                }

                if (barcode != null && this.BarcodeLenght > 0 && barcode.Length == this.BarcodeLenght || this.MissionOperation.MaximumQuantity == decimal.One)//16 => lunghezza matrice
                {
                    this.ShowNotification((Localized.Get("OperatorApp.BarcodeOperationConfirmed") + barcode), Services.Models.NotificationSeverity.Success);
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, 1, barcode);
                    quantity = 1;
                }
                else
                {
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value, barcode);
                }

                if (canComplete)
                {
                    await this.UpdateWeight(loadingUnitId, quantity, item.AverageWeight, type);
                    await this.PrintWeightAsync(itemId, (int?)quantity);

                    this.ShowNotification(Localized.Get("OperatorApp.OperationConfirmed"));
                }
                else
                {
                    this.ShowNotification(Localized.Get("OperatorApp.OperationCancelled"));
                    this.NavigationService.GoBackTo(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.WAIT,
                        "ConfirmOperationAsync");
                }

                //this.navigationService.GoBackTo(
                //    nameof(Utils.Modules.Operator),
                //    Utils.Modules.Operator.ItemOperations.WAIT);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                if (ex is MasWebApiException webEx)
                {
                    if (webEx.StatusCode == StatusCodes.Status403Forbidden)
                    {
                        this.ShowNotification(Localized.Get("General.ForbiddenOperation"), Services.Models.NotificationSeverity.Error);
                    }
                    else
                    {
                        var error = $"{Localized.Get("General.BadRequestTitle")}: ({webEx.StatusCode})";
                        this.ShowNotification(error, Services.Models.NotificationSeverity.Error);
                    }
                }
                else if (ex is System.Net.Http.HttpRequestException hEx)
                {
                    var error = $"{Localized.Get("General.BadRequestTitle")}: ({hEx.Message})";
                    this.ShowNotification(error, Services.Models.NotificationSeverity.Error);
                }
                else
                {
                    this.ShowNotification(ex);
                }

                this.IsBusyConfirmingOperation = false;
                this.IsOperationConfirmed = false;
            }
            finally
            {
                // Do not enable the interface. Wait for a new notification to arrive.
                this.IsWaitingForResponse = false;
                this.lastItemQuantityMessage = null;

                //this.lastMissionOperation = null;
                //this.lastMissionOperation = null;
            }
        }

        public override void Disappear()
        {
            this.IsVisibleBarcodeReader = false;
            this.BarcodeString = string.Empty;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBarcodeActive = this.barcodeReaderService.IsActive;
            this.IsVisibleBarcodeReader = false;
            this.BarcodeString = string.Empty;

            this.IsAddItem = false;

            this.CloseLine = true;
            this.FullCompartment = false;
            this.EmptyCompartment = false;

            // Setup only reserved for Tendaggi Paradiso
            this.IsCurrentDraperyItemFullyRequested = this.IsCurrentDraperyItem && this.MissionOperation.FullyRequested.HasValue && this.MissionOperation.FullyRequested.Value;

            await base.OnAppearedAsync();

            this.MeasureUnitDescription = string.Format(Resources.Localized.Get("OperatorApp.DrawerActivityRefillingQtyRefilled"), this.MeasureUnit);

            this.RaisePropertyChanged(nameof(this.MeasureUnitDescription));
        }

        public override void OnMisionOperationRetrieved()
        {
            if (this.MissionOperation != null)
            {
                if (this.MissionOperation.ItemCode.Contains("CONTENITORE"))
                {
                    this.CanPutBox = true;
                }
                else
                {
                    this.CanPutBox = false;
                }

                this.MissionRequestedQuantity = this.MissionOperation.RequestedQuantity - this.MissionOperation.DispatchedQuantity;
            }
            else
            {
                this.CanPutBox = false;
            }
            this.InputQuantity = this.MissionRequestedQuantity;
            base.InitializeInputQuantity();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.BarcodeString));
            this.barcodeReaderConfirmCommand?.RaiseCanExecuteChanged();

            this.fullOperationCommand.RaiseCanExecuteChanged();
            this.confirmOperationCommand.RaiseCanExecuteChanged();
            this.confirmPartialOperationCommand.RaiseCanExecuteChanged();
            this.putBoxCommand.RaiseCanExecuteChanged();
        }

        protected void ShowBarcodeReader()
        {
            this.IsVisibleBarcodeReader = true;
        }

        protected override void ShowOperationDetails()
        {
            this.NavigationService.Appear(
            nameof(Utils.Modules.Operator),
            Utils.Modules.Operator.ItemOperations.PUT_DETAILS,
            new DrawerActivityItemDetail
            {
                ItemCode = this.MissionOperation.ItemCode,
                ItemDescription = this.MissionOperation.ItemDescription,
                ListCode = this.MissionOperation.ItemListCode,
                ListDescription = this.MissionOperation.ItemListDescription,
                ListRow = this.MissionOperation.ItemListRowCode,
                Batch = this.MissionOperation.ItemListShipmentUnitCode,
                ProductionDate = this.MissionOperation.ItemProductionDate.ToString(),
                RequestedQuantity = this.MissionOperation.RequestedQuantity.ToString(),
            },
            trackCurrentView: true);
        }

        private bool CanPartiallyCompleteOnFullCompartment()
        {
            if (this.IsDoubleConfirmBarcodePut && string.IsNullOrEmpty(this.barcodeOk))
            {
                this.CanConfirmPartialOperation = false;
            }
            else
            {
                this.CanConfirmPartialOperation =
                    !this.IsWaitingForResponse
                    &&
                    this.MissionOperation != null
                    &&
                    !this.IsBusyAbortingOperation
                    &&
                    !this.IsBusyConfirmingOperation
                    &&
                    this.InputQuantity.HasValue
                    &&
                    this.InputQuantity.Value >= 0
                    &&
                    this.InputQuantity.Value != this.MissionRequestedQuantity
                    &&
                    !this.CanPutBox;
            }
            this.RaisePropertyChanged(nameof(this.CanConfirmPartialOperation));
            return this.CanConfirmPartialOperation;
        }

        private bool CanPutBoxes()
        {
            try
            {
                return this.MissionOperation != null
                        &&
                        !this.IsWaitingForResponse
                        &&
                        !this.IsBusyAbortingOperation
                        &&
                        !this.IsBusyConfirmingOperation
                        &&
                        this.InputQuantity.HasValue
                        &&
                        this.CanInputQuantity
                        &&
                        this.InputQuantity.Value == this.MissionRequestedQuantity
                        &&
                        this.CanPutBox;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task PutBoxAsync(string barcode)
        {
            System.Diagnostics.Debug.Assert(
                this.InputQuantity.HasValue,
                "The input quantity should have a value");

            try
            {
                this.IsBusyConfirmingOperation = true;
                this.IsWaitingForResponse = true;
                this.ClearNotifications();

                this.IsOperationConfirmed = true;

                bool canComplete = false;

                if (barcode != null)
                {
                    this.ShowNotification((Localized.Get("OperatorApp.BarcodeOperationConfirmed") + barcode), Services.Models.NotificationSeverity.Success);
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value, barcode);
                    this.Logger.Debug("Barcode: " + barcode);
                }
                else
                {
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value);
                }

                if (canComplete)
                {
                    this.ShowNotification(Localized.Get("OperatorApp.OperationConfirmed"));
                }
                else
                {
                    this.ShowNotification(Localized.Get("OperatorApp.OperationCancelled"));
                    this.NavigationService.GoBackTo(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.WAIT,
                        "PutBoxAsync");
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                if (ex is MasWebApiException webEx)
                {
                    if (webEx.StatusCode == StatusCodes.Status403Forbidden)
                    {
                        this.ShowNotification(Localized.Get("General.ForbiddenOperation"), Services.Models.NotificationSeverity.Error);
                    }
                    else
                    {
                        var error = $"{Localized.Get("General.BadRequestTitle")}: ({webEx.StatusCode})";
                        this.ShowNotification(error, Services.Models.NotificationSeverity.Error);
                    }
                }
                else if (ex is System.Net.Http.HttpRequestException hEx)
                {
                    var error = $"{Localized.Get("General.BadRequestTitle")}: ({hEx.Message})";
                    this.ShowNotification(error, Services.Models.NotificationSeverity.Error);
                }
                else
                {
                    this.ShowNotification(ex);
                }
                this.IsBusyConfirmingOperation = false;
                this.IsOperationConfirmed = false;
            }
            finally
            {
                // Do not enable the interface. Wait for a new notification to arrive.
                this.IsWaitingForResponse = false;
                this.lastItemQuantityMessage = null;
            }
        }

        #endregion
    }
}
