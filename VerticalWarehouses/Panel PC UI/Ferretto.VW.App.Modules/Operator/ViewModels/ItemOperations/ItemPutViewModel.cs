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
    public class ItemPutViewModel : BaseItemOperationMainViewModel
    {
        #region Fields

        private DelegateCommand confirmOperationCommand;

        private DelegateCommand fullOperationCommand;

        #endregion

        #region Constructors

        public ItemPutViewModel(
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

        public ICommand ConfirmOperationCommand =>
            this.confirmOperationCommand
            ??
            (this.confirmOperationCommand = new DelegateCommand(
                async () => await this.ConfirmOperationAsync(),
                this.CanConfirmOperation));

        public ICommand FullOperationCommand =>
                    this.fullOperationCommand
            ??
            (this.fullOperationCommand = new DelegateCommand(
                async () => await this.PartiallyCompleteOnFullCompartmentAsync(),
                this.CanPartiallyCompleteOnFullCompartmen));

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
                !this.IsOperationConfirmed
                &&
                this.InputQuantity.HasValue
                &&
                this.InputQuantity.Value >= this.MissionRequestedQuantity;
        }

        public Task CommandUserActionAsync(UserActionEventArgs userAction)
        {
            // do nothing
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
                this.MissionRequestedQuantity = this.MissionOperation.RequestedQuantity - this.MissionOperation.DispatchedQuantity;
            }
            this.InputQuantity = this.MissionRequestedQuantity;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.fullOperationCommand.RaiseCanExecuteChanged();
            this.confirmOperationCommand.RaiseCanExecuteChanged();
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
                this.InputQuantity.Value < this.MissionRequestedQuantity;
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
                    this.ShowOperationCanceledMessage();
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

        #endregion
    }
}
