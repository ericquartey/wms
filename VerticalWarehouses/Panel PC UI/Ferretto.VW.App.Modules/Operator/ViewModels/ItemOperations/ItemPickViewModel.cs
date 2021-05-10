using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemPickViewModel : BaseItemOperationMainViewModel, IOperationalContextViewModel
    {
        #region Fields

        private readonly IMachineItemsWebService itemsWebService;

        private bool canConfirm;

        private bool canConfirmOnEmpty;

        private bool canPickBox;

        private DelegateCommand emptyOperationCommand;

        private string measureUnitTxt;

        private DelegateCommand pickBoxCommand;

        #endregion

        //private DelegateCommand signallingDefectCommand;

        #region Constructors

        public ItemPickViewModel(
            ILaserPointerService deviceService,
            IMachineAreasWebService areasWebService,
            IMachineIdentityWebService machineIdentityWebService,
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
            IAuthenticationService authenticationService)
            : base(
                  deviceService,
                  areasWebService,
                  machineIdentityWebService,
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
                  authenticationService)
        {
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
        }

        #endregion

        #region Properties

        public override string ActiveContextName => OperationalContext.ItemPick.ToString();

        public bool CanConfirm
        {
            get => this.canConfirm;
            set => this.SetProperty(ref this.canConfirm, value);
        }

        public bool CanConfirmOnEmpty
        {
            get => this.canConfirmOnEmpty;
            set => this.SetProperty(ref this.canConfirmOnEmpty, value);
        }

        public bool CanPickBox
        {
            get => this.canPickBox;
            set => this.SetProperty(ref this.canPickBox, value && this.IsBoxEnabled, this.RaiseCanExecuteChanged);
        }

        public ICommand EmptyOperationCommand =>
                    this.emptyOperationCommand
            ??
            (this.emptyOperationCommand = new DelegateCommand(
                async () => await this.PartiallyCompleteOnEmptyCompartmentAsync(),
                this.CanPartiallyCompleteOnEmptyCompartment));

        public override EnableMask EnableMask => EnableMask.Any;

        public string MeasureUnitTxt
        {
            get => this.measureUnitTxt;
            set => this.SetProperty(ref this.measureUnitTxt, value, this.RaiseCanExecuteChanged);
        }

        public ICommand PickBoxCommand =>
            this.pickBoxCommand
            ??
            (this.pickBoxCommand = new DelegateCommand(
                async () => await this.PickBoxAsync("0"),
                this.CanPickBoxes));

        #endregion

        //public ICommand SignallingDefectCommand =>promag
        //    this.signallingDefectCommand
        //    ??
        //    (this.signallingDefectCommand = new DelegateCommand(
        //        /*async*/ () => /*await*/ this.SignallingDefect(),
        //        this.CanOpenSignallingDefect));

        #region Methods

        public async Task CommandUserActionAsync(UserActionEventArgs userAction)
        {
            if (userAction is null)
            {
                return;
            }

            if (this.CanPickBoxes() && userAction.UserAction == UserAction.VerifyItem)
            {
                await this.PickBoxAsync(userAction.Code);
                return;
            }

            if (this.CanConfirmOperation() && userAction.UserAction == UserAction.VerifyItem)
            {
                await base.CommandUserActionAsync(userAction);
                return;
            }
        }

        public override void Disappear()
        {
            //if (this.lastMissionOperation != null && this.MissionOperation != null)
            //{
            //    this.lastMissionOperation.RequestedQuantity = this.InputQuantity.Value;
            //}

            //if (this.lastSelectedCompartmentDetail != null && this.AvailableQuantity.HasValue)
            //{
            //    this.lastSelectedCompartmentDetail.Stock = this.AvailableQuantity.Value;
            //}

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsAddItem = false;
            this.CanInputAvailableQuantity = true;
            this.CanInputQuantity = true;
            this.CloseLine = true;
            this.FullCompartment = false;
            this.EmptyCompartment = false;
            this.RaisePropertyChanged(nameof(this.CanInputAvailableQuantity));
            this.RaisePropertyChanged(nameof(this.CanInputQuantity));

            this.Compartments = null;
            this.SelectedCompartment = null;

            this.MeasureUnitTxt = string.Format(Resources.Localized.Get("OperatorApp.PickedQuantity"), this.MeasureUnit);

            await base.OnAppearedAsync();

            //this.SetLastQuantity();
        }

        public override void OnMisionOperationRetrieved()
        {
            if (this.MissionOperation != null)
            {
                if (this.MissionOperation != null)
                {
                    if (this.MissionOperation.ItemCode.Contains("CONTENITORE"))
                    {
                        this.CanPickBox = true;
                    }
                    else
                    {
                        this.CanPickBox = false;
                    }

                    this.MissionRequestedQuantity = this.MissionOperation.RequestedQuantity - this.MissionOperation.DispatchedQuantity;
                }
                else
                {
                    this.CanPickBox = false;
                }

                this.InputQuantity = this.MissionRequestedQuantity;
                base.InitializeInputQuantity();
                //this.AvailableQuantity = this.MissionRequestedQuantity;

                this.RaisePropertyChanged(nameof(this.InputQuantity));
                this.RaisePropertyChanged(nameof(this.AvailableQuantity));
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.pickBoxCommand?.RaiseCanExecuteChanged();

            this.emptyOperationCommand?.RaiseCanExecuteChanged();

            this.MeasureUnitTxt = string.Format(Resources.Localized.Get("OperatorApp.PickedQuantity"), this.MeasureUnit);

            this.emptyOperationCommand?.RaiseCanExecuteChanged();
            this.emptyOperationCommand.RaiseCanExecuteChanged();

            //this.signallingDefectCommand.RaiseCanExecuteChanged();
        }

        protected override void ShowOperationDetails()
        {
            this.NavigationService.Appear(
               nameof(Utils.Modules.Operator),
               Utils.Modules.Operator.ItemOperations.PICK_DETAILS,
               null,
               trackCurrentView: true);
        }

        //private bool CanOpenSignallingDefect()
        //{
        //    return this.IsCurrentDraperyItem;
        //}

        private bool CanPartiallyCompleteOnEmptyCompartment()
        {
            try
            {
                this.CanConfirm =
                this.MissionOperation != null
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
                !this.CanPickBox;

                this.RaisePropertyChanged(nameof(this.CanConfirm));

                this.CanConfirmPartialOperation =
                    this.MissionOperation != null
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
                    this.InputQuantity.Value >= 0
                    &&
                    this.InputQuantity.Value != this.MissionRequestedQuantity
                    &&
                    this.InputQuantity.Value <= this.AvailableQuantity
                    &&
                    !this.CanPickBox;

                this.RaisePropertyChanged(nameof(this.CanConfirmPartialOperation));
            }
            catch (Exception)
            {
            }
            //return this.CanConfirm;
            return false;
        }

        private bool CanPickBoxes()
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
                        this.CanPickBox;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task PartiallyCompleteOnEmptyCompartmentAsync()
        {
            this.IsWaitingForResponse = true;
            this.IsOperationConfirmed = true;

            try
            {
                var item = await this.itemsWebService.GetByIdAsync(this.MissionOperation.ItemId);
                var loadingUnitId = this.Mission.LoadingUnit.Id;
                var type = this.MissionOperation.Type;
                var quantity = this.InputQuantity.Value;

                var canComplete = await this.MissionOperationsService.PartiallyCompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value, 0, null, false, false);
                if (!canComplete)
                {
                    this.ShowNotification(Localized.Get("OperatorApp.OperationCancelled"));
                    this.NavigationService.GoBackTo(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.WAIT,
                        "PartiallyCompleteOnEmptyCompartmentAsync");
                }
                else
                {
                    await this.UpdateWeight(loadingUnitId, quantity, item.AverageWeight, type);
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.IsOperationConfirmed = false;
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
                this.lastItemQuantityMessage = null;
            }
        }

        // To use with drapery item
        private async Task PickBox_New_Async(string barcode)
        {
            System.Diagnostics.Debug.Assert(
                this.InputQuantity.HasValue,
                "The input quantity should have a value");

            // Show the confirm view for the drapery item
            if (this.IsCurrentDraperyItem)
            {
                this.ShowDraperyItemConfirmView(
                    barcode,
                    isPartiallyConfirmOperation: false);

                return;
            }

            try
            {
                this.IsBusyConfirmingOperation = true;
                this.IsWaitingForResponse = true;
                this.ClearNotifications();

                this.IsOperationConfirmed = true;

                bool canComplete = false;

                if (barcode != null)
                {
                    this.ShowNotification(Localized.Get("OperatorApp.BarcodeOperationConfirmed") + barcode, Services.Models.NotificationSeverity.Success);
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value, barcode);
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
                        "PickBox_New_Async");
                }

                //this.navigationService.GoBackTo(
                //    nameof(Utils.Modules.Operator),
                //    Utils.Modules.Operator.ItemOperations.WAIT);
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
                this.lastItemQuantityMessage = null;

                //this.lastMissionOperation = null;
                //this.lastMissionOperation = null;
            }
        }

        private async Task PickBoxAsync(string barcode)
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
                        "PickBoxAsync");
                }

                //this.navigationService.GoBackTo(
                //    nameof(Utils.Modules.Operator),
                //    Utils.Modules.Operator.ItemOperations.WAIT);
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
                this.lastItemQuantityMessage = null;

                //this.lastMissionOperation = null;
                //this.lastMissionOperation = null;
            }
        }

        //private /*async Task*/ void SignallingDefect()
        //{
        //    this.Logger.Debug("Signalling defect call....");

        //    this.NavigationService.Appear(
        //        nameof(Utils.Modules.Operator),
        //        Utils.Modules.Operator.ItemOperations.SIGNALLINGDEFECT,
        //        this.MissionOperation);
        //    //this.MissionOperation,
        //    //trackCurrentView: true);
        //}

        //private void SetLastQuantity()
        //{
        //    try
        //    {
        //        if (this.lastMissionOperation == null && this.MissionOperation != null)
        //        {
        //            this.lastMissionOperation = this.MissionOperation;
        //            this.lastMissionOperation.RequestedQuantity = this.MissionRequestedQuantity;
        //        }
        //        else if (this.MissionOperation != null)
        //        {
        //            if (this.lastMissionOperation.MissionId == this.MissionOperation.MissionId && this.lastMissionOperation.ItemCode == this.MissionOperation.ItemCode)
        //            {
        //                if (this.lastMissionOperation.RequestedQuantity != this.MissionRequestedQuantity)
        //                {
        //                    //this.MissionOperation.RequestedQuantity = this.lastMissionOperation.RequestedQuantity;
        //                    //this.RaisePropertyChanged(nameof(this.MissionOperation));
        //                    this.InputQuantity = this.lastMissionOperation.RequestedQuantity;
        //                    this.RaisePropertyChanged(nameof(this.InputQuantity));
        //                }
        //            }
        //            else
        //            {
        //                this.lastMissionOperation = this.MissionOperation;
        //                this.lastMissionOperation.RequestedQuantity = this.MissionRequestedQuantity;
        //            }
        //        }

        //        if (this.lastSelectedCompartmentDetail == null && this.SelectedCompartmentDetail != null && this.MissionOperation != null)
        //        {
        //            this.lastSelectedCompartmentDetail = this.SelectedCompartmentDetail;
        //        }
        //        else if (this.SelectedCompartmentDetail != null && this.MissionOperation != null)
        //        {
        //            if (this.lastSelectedCompartmentDetail.ItemCode == this.SelectedCompartmentDetail.ItemCode)
        //            {
        //                if (this.lastMissionOperation.CompartmentId == this.MissionOperation.CompartmentId && this.lastMissionOperation.MissionId == this.MissionOperation.MissionId)
        //                {
        //                    if (this.lastSelectedCompartmentDetail.Stock != this.SelectedCompartmentDetail.Stock)
        //                    {
        //                        //this.SelectedCompartmentDetail.Stock = this.lastSelectedCompartmentDetail.Stock;
        //                        //this.RaisePropertyChanged(nameof(this.SelectedCompartmentDetail));
        //                        this.AvailableQuantity = this.lastSelectedCompartmentDetail.Stock;
        //                        this.RaisePropertyChanged(nameof(this.AvailableQuantity));
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                this.lastSelectedCompartmentDetail = this.SelectedCompartmentDetail;
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        //
        //    }
        //}

        private void ShowDraperyItemConfirmView(string barcode, bool isPartiallyConfirmOperation)
        {
            this.Logger.Debug($"Show the confirm view for drapery item {this.ItemId}, description {this.MissionOperation.ItemDescription}");

            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemOperations.DRAPERYCONFIRM,
                new ItemDraperyDataConfirm
                {
                    MissionId = this.MissionOperation.Id,
                    ItemDescription = this.MissionOperation.ItemDescription,
                    AvailableQuantity = this.AvailableQuantity.Value,
                    MissionRequestedQuantity = this.MissionRequestedQuantity,
                    InputQuantity = this.InputQuantity,
                    CanInputQuantity = this.CanInputQuantity,
                    QuantityIncrement = this.QuantityIncrement,
                    QuantityTolerance = this.QuantityTolerance,
                    MeasureUnitTxt = this.MeasureUnitTxt,
                    Barcode = barcode,
                    IsPartiallyCompleteOperation = isPartiallyConfirmOperation,
                },
                trackCurrentView: true);
        }

        #endregion
    }
}
