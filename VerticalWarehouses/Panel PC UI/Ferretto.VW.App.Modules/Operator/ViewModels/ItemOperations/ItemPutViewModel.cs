using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemPutViewModel : BaseItemOperationMainViewModel, IOperationalContextViewModel
    {
        #region Fields

        private bool canPutBox;

        private bool confirmOperation;

        private DelegateCommand confirmOperationCommand;

        private bool confirmPartialOperation;

        private DelegateCommand confirmPartialOperationCommand;

        private DelegateCommand fullOperationCommand;

        private DelegateCommand putBoxCommand;

        #endregion

        #region Constructors

        public ItemPutViewModel(
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
            IDialogService dialogService)
            : base(
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
                  dialogService)
        {
        }

        #endregion

        #region Properties

        public override string ActiveContextName => OperationalContext.ItemPut.ToString();

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
                async () => await this.ConfirmOperationAsync("0"),
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
                async () => await this.ConfirmOperationAsync("0"),
                this.CanConfirmPartialOperationPut));

        public ICommand FullOperationCommand =>
                    this.fullOperationCommand
            ??
            (this.fullOperationCommand = new DelegateCommand(
                async () => await this.PartiallyCompleteOnFullCompartmentAsync(),
                this.CanPartiallyCompleteOnFullCompartmen));

        public ICommand PutBoxCommand =>
            this.putBoxCommand
            ??
            (this.putBoxCommand = new DelegateCommand(
                async () => await this.PutBoxAsync("0"),
                this.CanPutBoxes));

        #endregion

        #region Methods

        public bool CanConfirmOperationPut()
        {
            try
            {
                this.confirmOperation = this.MissionOperation != null &&
                this.InputQuantity.Value == this.MissionRequestedQuantity &&
                !this.IsOperationCanceled;

                this.confirmPartialOperation = this.MissionOperation != null &&
                    this.InputQuantity.Value != this.MissionRequestedQuantity &&
                    !this.IsOperationCanceled;

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
               this.InputQuantity.Value >= this.MissionRequestedQuantity
               &&
               !this.CanPutBox;
        }

        public bool CanConfirmPartialOperationPut()
        {
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
               this.InputQuantity.Value > this.MissionRequestedQuantity
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

            if (this.CanConfirmOperationPut() && userAction.UserAction == UserAction.VerifyItem)
            {
                await base.CommandUserActionAsync(userAction);
                return;
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

                bool canComplete = false;

                if (barcode != null && this.BarcodeLenght > 0 && barcode.Length == this.BarcodeLenght)//16 => lunghezza matrice
                {
                    this.ShowNotification((Localized.Get("OperatorApp.BarcodeOperationConfirmed") + barcode), Services.Models.NotificationSeverity.Success);
                    canComplete = await this.MissionOperationsService.CompleteAsync(this.MissionOperation.Id, 1, barcode);
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
                        Utils.Modules.Operator.ItemOperations.WAIT);
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

                //this.lastMissionOperation = null;
                //this.lastMissionOperation = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
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
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.fullOperationCommand.RaiseCanExecuteChanged();
            this.confirmOperationCommand.RaiseCanExecuteChanged();
            this.confirmPartialOperationCommand.RaiseCanExecuteChanged();
            this.putBoxCommand.RaiseCanExecuteChanged();
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

        private bool CanPartiallyCompleteOnFullCompartmen()
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
                this.InputQuantity.HasValue
                &&
                this.InputQuantity.Value >= 0
                &&
                this.InputQuantity.Value < this.MissionRequestedQuantity
                &&
                !this.CanPutBox;
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

        private async Task PartiallyCompleteOnFullCompartmentAsync()
        {
            this.IsWaitingForResponse = true;
            this.IsOperationConfirmed = true;

            try
            {
                var canComplete = await this.MissionOperationsService.PartiallyCompleteAsync(this.MissionOperation.Id, this.InputQuantity.Value);
                if (!canComplete)
                {
                    this.ShowNotification(Localized.Get("OperatorApp.OperationCancelled"));
                    this.NavigationService.GoBackTo(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.WAIT);
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
                        Utils.Modules.Operator.ItemOperations.WAIT);
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

        #endregion
    }
}
