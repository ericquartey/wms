using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.Controls.WPF;
using Ferretto.VW.App.Accessories;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.AlphaNumericBar;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public abstract class BaseItemOperationMainViewModel : BaseItemOperationViewModel, IDataErrorInfo, IOperationalContextViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private AlphaNumericBarDriver alphaNumericBarDriver;

        private Bay bay;

        private IEnumerable<TrayControlCompartment> compartments;

        private DelegateCommand confirmOperationCanceledCommand;

        private DelegateCommand confirmOperationCommand;

        private string inputItemCode;

        private string inputLot;

        private double? inputQuantity;

        private string inputSerialNumber;

        private bool isBusyAbortingOperation;

        private bool isBusyConfirmingOperation;

        private bool isInputQuantityValid;

        private bool isItemCodeValid;

        private bool isItemLotValid;

        private bool isItemSerialNumberValid;

        private bool isOperationCanceled;

        private double loadingUnitDepth;

        private double loadingUnitWidth;

        private SubscriptionToken missionToken;

        private TrayControlCompartment selectedCompartment;

        private DelegateCommand showDetailsCommand;

        #endregion

        #region Constructors

        public BaseItemOperationMainViewModel(
            IMachineItemsWebService itemsWebService,
            IBayManager bayManager,
            IEventAggregator eventAggregator,
            IMissionOperationsService missionOperationsService,
            IDialogService dialogService)
            : base(itemsWebService, bayManager, missionOperationsService, dialogService)
        {
            this.eventAggregator = eventAggregator;

            this.CompartmentColoringFunction = (compartment, selectedCompartment) => compartment == selectedCompartment ? "#0288f7" : "#444444";

            _ = this.AlphaNumericBarConfigureAsync(); // TODO: async calls should be performed in the OnAppearAsync() method
        }

        #endregion

        #region Properties

        public abstract string ActiveContextName { get; }

        public Func<IDrawableCompartment, IDrawableCompartment, string> CompartmentColoringFunction { get; }

        public IEnumerable<TrayControlCompartment> Compartments
        {
            get => this.compartments;
            set => this.SetProperty(ref this.compartments, value);
        }

        public ICommand ConfirmOperationCanceledCommand =>
            this.confirmOperationCanceledCommand
            ??
            (this.confirmOperationCanceledCommand = new DelegateCommand(
                async () => await this.ConfirmOperationCanceledAsync(),
                this.CanConfirmOperationCanceled));

        public ICommand ConfirmOperationCommand =>
            this.confirmOperationCommand
            ??
            (this.confirmOperationCommand = new DelegateCommand(
                async () => await this.ConfirmOperationAsync(),
                this.CanConfirmOperation));

        public override EnableMask EnableMask => EnableMask.Any;

        public string Error => string.Join(
                    Environment.NewLine,
                    this[nameof(this.InputQuantity)],
                    this[nameof(this.InputLot)],
                    this[nameof(this.InputItemCode)],
                    this[nameof(this.InputSerialNumber)]);

        public string InputItemCode
        {
            get => this.inputItemCode;
            protected set => this.SetProperty(
                ref this.inputItemCode,
                value,
                () => this.IsItemCodeValid = this[nameof(this.InputItemCode)] is null);
        }

        public string InputLot
        {
            get => this.inputLot;
            protected set => this.SetProperty(
                ref this.inputLot,
                value,
                () => this.IsItemLotValid = this[nameof(this.InputLot)] is null);
        }

        public double? InputQuantity
        {
            get => this.inputQuantity;
            set => this.SetProperty(
                ref this.inputQuantity,
                value,
                () =>
                {
                    this.IsInputQuantityValid = this[nameof(this.InputQuantity)] is null;
                    this.RaiseCanExecuteChanged();
                });
        }

        public string InputSerialNumber
        {
            get => this.inputSerialNumber;
            protected set => this.SetProperty(
                ref this.inputSerialNumber,
                value,
                () => this.IsItemSerialNumberValid = this[nameof(this.InputSerialNumber)] is null);
        }

        public bool IsBaySideBack => this.bay?.Side == MAS.AutomationService.Contracts.WarehouseSide.Back;

        public bool IsBusyAbortingOperation
        {
            get => this.isBusyAbortingOperation;
            set => this.SetProperty(ref this.isBusyAbortingOperation, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBusyConfirmingOperation
        {
            get => this.isBusyConfirmingOperation;
            set => this.SetProperty(ref this.isBusyConfirmingOperation, value, this.RaiseCanExecuteChanged);
        }

        public bool IsInputQuantityValid
        {
            get => this.isInputQuantityValid;
            protected set => this.SetProperty(ref this.isInputQuantityValid, value);
        }

        public bool IsItemCodeValid
        {
            get => this.isItemCodeValid;
            protected set => this.SetProperty(ref this.isItemCodeValid, value);
        }

        public bool IsItemLotValid
        {
            get => this.isItemLotValid;
            protected set => this.SetProperty(ref this.isItemLotValid, value);
        }

        public bool IsItemSerialNumberValid
        {
            get => this.isItemSerialNumberValid;
            protected set => this.SetProperty(ref this.isItemSerialNumberValid, value);
        }

        public bool IsOperationCanceled
        {
            get => this.isOperationCanceled;
            set => this.SetProperty(ref this.isOperationCanceled, value);
        }

        public double LoadingUnitDepth
        {
            get => this.loadingUnitDepth;
            set => this.SetProperty(ref this.loadingUnitDepth, value, this.RaiseCanExecuteChanged);
        }

        public double LoadingUnitWidth
        {
            get => this.loadingUnitWidth;
            set => this.SetProperty(ref this.loadingUnitWidth, value, this.RaiseCanExecuteChanged);
        }

        public TrayControlCompartment SelectedCompartment
        {
            get => this.selectedCompartment;
            set => this.SetProperty(ref this.selectedCompartment, value);
        }

        public ICommand ShowDetailsCommand =>
            this.showDetailsCommand
            ??
            (this.showDetailsCommand = new DelegateCommand(this.ShowOperationDetails));

        protected bool IsOperationConfirmed { get; set; }

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.InputLot):
                        {
                            if (this.InputLot != null && this.InputLot != this.MissionOperation?.Lot)
                            {
                                return columnName;
                            }

                            break;
                        }

                    case nameof(this.InputQuantity):
                        {
                            if (this.InputQuantity != null && this.InputQuantity != this.MissionOperation?.RequestedQuantity)
                            {
                                return columnName;
                            }

                            break;
                        }

                    case nameof(this.InputItemCode):
                        {
                            if (this.InputItemCode != null
                                &&
                                this.MissionOperation?.ItemCode != null
                                &&
                                this.InputItemCode != this.MissionOperation.ItemCode)
                            {
                                return columnName;
                            }

                            break;
                        }

                    case nameof(this.InputSerialNumber):
                        {
                            if (this.InputSerialNumber != null
                                &&
                                this.MissionOperation?.SerialNumber != null
                                &&
                                this.InputSerialNumber != this.MissionOperation.SerialNumber)
                            {
                                return columnName;
                            }

                            break;
                        }
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public virtual bool CanConfirmOperation()
        {
            return
                !this.IsWaitingForResponse
                &&
                this.MissionOperation != null
                &&
                !this.IsBusyAbortingOperation
                &&
                !this.IsBusyConfirmingOperation
                &&
                !this.IsOperationConfirmed
                &&
                !this.isOperationCanceled
                &&
                this.InputQuantity.HasValue
                &&
                this.InputQuantity.Value >= 0
                &&
                this.InputQuantity.Value == this.MissionOperation.RequestedQuantity;
        }

        public virtual bool CanConfirmOperationCanceled()
        {
            return
                !this.IsWaitingForResponse
                &&
                !this.IsOperationConfirmed
                &&
                this.isOperationCanceled;
        }

        public Task CommandUserActionAsync(UserActionEventArgs e)
        {
            if (e is null)
            {
                return Task.CompletedTask;
            }

            if (!Enum.TryParse<UserAction>(e.UserAction, out var userAction))
            {
                return Task.CompletedTask;
            }

            switch (userAction)
            {
                case UserAction.VerifyItem:
                    {
                        var itemCode = e.GetItemCode();
                        if (itemCode is null)
                        {
                            this.ShowNotification(
                                string.Format(Resources.Localized.Get("OperatorApp.BarcodeDoesNotContainTheItemCode"), e.Code),
                                Services.Models.NotificationSeverity.Warning);

                            return Task.CompletedTask;
                        }

                        this.InputItemCode = itemCode;

                        this.InputQuantity = e.GetItemQuantity() ?? this.InputQuantity;

                        this.InputSerialNumber = e.GetItemSerialNumber() ?? this.InputSerialNumber;

                        this.InputLot = e.GetItemLot() ?? this.InputLot;
                    }

                    break;
            }

            return Task.CompletedTask;
        }

        public async Task ConfirmOperationAsync()
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

                var canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value);
                if (canComplete)
                {
                    this.ShowNotification(Localized.Get("OperatorApp.OperationConfirmed"));
                }
                else
                {
                    this.ShowOperationCanceledMessage();
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.IsBusyConfirmingOperation = false;
                this.IsOperationConfirmed = false;
            }
            finally
            {
                // Do not enable the interface. Wait for a new notification to arrive.
                this.IsWaitingForResponse = false;
            }
        }

        public async Task ConfirmOperationCanceledAsync()
        {
            try
            {
                this.IsBusyConfirmingOperation = true;
                this.IsWaitingForResponse = true;
                this.ClearNotifications();

                this.ShowNotification(Localized.Get("OperatorApp.OperationCancelledConfirmed"));

                // ?????????????? this.NavigationService.GoBack();
                // this.MissionOperation = null;
                // this.Mission = null;
                await this.MissionOperationsService.RefreshAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
                this.IsBusyConfirmingOperation = false;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        public override void Disappear()
        {
            this.missionToken?.Dispose();
            this.missionToken = null;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = false;
            this.IsBusyAbortingOperation = false;
            this.IsBusyConfirmingOperation = false;
            this.IsOperationConfirmed = false;
            this.IsOperationCanceled = false;
            this.InputQuantity = null;
            this.SelectedCompartment = null;

            await base.OnAppearedAsync();

            this.bay = await this.BayManager.GetBayAsync();

            this.RaisePropertyChanged(nameof(this.IsBaySideBack));

            this.missionToken = this.missionToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<MissionChangedEventArgs>>()
                    .Subscribe(
                        async e => await this.OnMissionChangedAsync(e),
                        ThreadOption.UIThread,
                        false);

            this.GetLoadingUnitDetails();

            await this.AlphaNumericBarSendMessageAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.confirmOperationCommand?.RaiseCanExecuteChanged();
            this.showDetailsCommand?.RaiseCanExecuteChanged();
            this.confirmOperationCanceledCommand?.RaiseCanExecuteChanged();
        }

        protected void ShowOperationCanceledMessage()
        {
            this.IsOperationCanceled = true;
            this.CanInputQuantity = false;
            this.IsWaitingForResponse = false;
            this.IsBusyConfirmingOperation = false;
            this.IsOperationConfirmed = false;

            var msg = this.GetNoLongerOperationMessageByType();
            this.DialogService.ShowMessage(msg, Localized.Get("OperatorApp.OperationCancelled"), DialogType.Error, DialogButtons.OK);
            this.ShowNotification(msg, Services.Models.NotificationSeverity.Warning);
            this.HideNavigationBack();
        }

        protected abstract void ShowOperationDetails();

        private static IEnumerable<TrayControlCompartment> MapCompartments(IEnumerable<CompartmentMissionInfo> compartmentsFromMission)
        {
            try
            {
                return compartmentsFromMission
                    .Where(c =>
                        c.Width.HasValue
                        ||
                        c.Depth.HasValue
                        ||
                        c.XPosition.HasValue
                        ||
                        c.YPosition.HasValue)
                    .Select(c => new TrayControlCompartment
                    {
                        Depth = c.Depth.Value,
                        Id = c.Id,
                        Width = c.Width.Value,
                        XPosition = c.XPosition.Value,
                        YPosition = c.YPosition.Value,
                    });
            }
            catch (Exception)
            {
                return Array.Empty<TrayControlCompartment>();
            }
        }

        private async Task AlphaNumericBarConfigureAsync()
        {
            try
            {
                var accessories = await this.BayManager.GetBayAccessoriesAsync();

                if (accessories is null)
                {
                    return;
                }

                var alphaNumericBar = accessories.AlphaNumericBar;
                if (alphaNumericBar.IsEnabledNew)
                {
                    this.alphaNumericBarDriver = new AlphaNumericBarDriver();

                    var ipAddress = alphaNumericBar.IpAddress;
                    var port = alphaNumericBar.TcpPort;
                    var size = (MAS.DataModels.AlphaNumericBarSize)alphaNumericBar.Size;

                    this.alphaNumericBarDriver.Configure(ipAddress, port, size);
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private async Task AlphaNumericBarSendMessageAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                if (this.alphaNumericBarDriver is null)
                {
                    return;
                }

                if (this.MissionOperation is null)
                {
                    await this.alphaNumericBarDriver.SetEnabledAsync(false); // no mission, then switch off the alpha numeric bar
                }
                else
                {
                    var message = "?";
                    var arrowPosition = this.alphaNumericBarDriver.CalculateArrowPosition(this.loadingUnitWidth, this.selectedCompartment is null ? 0 : this.selectedCompartment.XPosition.Value);
                    await this.alphaNumericBarDriver.SetAndWriteArrowAsync(arrowPosition, true);        // show the arrow in the rigth position

                    switch (this.MissionOperation.Type)
                    {
                        case MissionOperationType.Pick:
                            message = "-";
                            break;

                        case MissionOperationType.Put:
                            message = "+";
                            break;
                    }

                    message += this.MissionOperation.RequestedQuantity + " " + this.MissionOperation.ItemCode + " " + this.MissionOperation.ItemDescription;

                    var offset = this.alphaNumericBarDriver.CalculateOffset(arrowPosition + 6, message);
                    if (offset > 0)
                    {
                        await this.alphaNumericBarDriver.SetAndWriteMessageAsync(message, offset, false);
                    }
                    else if (offset == -1)
                    {
                        await this.alphaNumericBarDriver.SetAndWriteMessageScrollAsync(message, 0, arrowPosition, false);
                    }
                    else
                    {
                        var start = arrowPosition + 6;
                        await this.alphaNumericBarDriver.SetAndWriteMessageScrollAsync(message, start, (this.alphaNumericBarDriver.NumberOfLeds - start) / 6, false);
                    }
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void GetLoadingUnitDetails()
        {
            if (this.Mission is null || this.Mission.LoadingUnit is null)
            {
                this.Compartments = null;
                this.SelectedCompartment = null;
            }
            else
            {
                this.LoadingUnitWidth = this.Mission.LoadingUnit.Width;
                this.LoadingUnitDepth = this.Mission.LoadingUnit.Depth;

                this.Compartments = MapCompartments(this.Mission.LoadingUnit.Compartments);
                this.SelectedCompartment = this.Compartments.SingleOrDefault(c =>
                    c.Id == this.MissionOperation.CompartmentId);
            }
        }

        private string GetNoLongerOperationMessageByType()
        {
            var noLongerOperationMsg = string.Empty;
            switch (this.MissionOperation.Type)
            {
                case MissionOperationType.Pick:
                    noLongerOperationMsg = Localized.Get("OperatorApp.IfPickedItemsPutThemBackInTheOriginalCompartment");
                    break;

                case MissionOperationType.Put:
                    noLongerOperationMsg = Localized.Get("OperatorApp.RemoveAnySpilledItemsFromCompartment");
                    break;

                case MissionOperationType.Inventory:
                    noLongerOperationMsg = Localized.Get("OperatorApp.InventoryOperationCancelled");
                    break;

                default:
                    break;
            }

            return noLongerOperationMsg;
        }

        private void HideNavigationBack()
        {
            switch (this.MissionOperation.Type)
            {
                case MissionOperationType.Pick:
                    this.IsBackNavigationAllowed = false;
                    break;

                case MissionOperationType.Put:
                    this.IsBackNavigationAllowed = false;
                    break;

                default:
                    break;
            }
        }

        private async Task OnMissionChangedAsync(MissionChangedEventArgs e)
        {
            if (this.IsOperationConfirmed || this.IsOperationCanceled)
            {
                this.IsOperationConfirmed = false;

                await this.RetrieveMissionOperationAsync();

                this.GetLoadingUnitDetails();
            }

            _ = this.AlphaNumericBarSendMessageAsync();
            this.IsBusyConfirmingOperation = false;
            this.IsWaitingForResponse = false;
        }

        #endregion
    }
}
