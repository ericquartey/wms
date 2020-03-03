using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemPickViewModel : BaseItemOperationMainViewModel, IOperationalContextViewModel
    {
        #region Fields

        private bool canConfirmOnEmpty;

        private DelegateCommand emptyOperationCommand;

        #endregion

        #region Constructors

        public ItemPickViewModel(
            IMachineItemsWebService itemsWebService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            IDialogService dialogService)
            : base(itemsWebService, bayManager, eventAggregator, missionOperationsService, dialogService)
        {
        }

        #endregion

        #region Properties

        public string ActiveContextName => OperationalContext.ItemPick.ToString();

        public bool CanConfirmOnEmpty
        {
            get => this.canConfirmOnEmpty;
            set => this.SetProperty(ref this.canConfirmOnEmpty, value);
        }

        public ICommand EmptyOperationCommand =>
                    this.emptyOperationCommand
            ??
            (this.emptyOperationCommand = new DelegateCommand(
                async () => await this.PartiallyCompleteOnEmptyCompartmentAsync(),
                this.CanPartiallyCompleteOnEmptyCompartment));

        public override EnableMask EnableMask => EnableMask.Any;

        #endregion

        #region Methods

        public Task CommandUserActionAsync(UserActionEventArgs userAction)
        {
            // do nothing
            return Task.CompletedTask;
        }

        public override Task OnAppearedAsync()
        {
            this.Compartments = null;
            this.SelectedCompartment = null;

            return base.OnAppearedAsync();
        }

        public override void OnMisionOperationRetrieved()
        {
            this.InputQuantity = this.MissionOperation.RequestedQuantity - this.MissionOperation.DispatchedQuantity;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.emptyOperationCommand.RaiseCanExecuteChanged();
        }

        protected override void ShowOperationDetails()
        {
            this.NavigationService.Appear(
               nameof(Utils.Modules.Operator),
               Utils.Modules.Operator.ItemOperations.PICK_DETAILS,
               null,
               trackCurrentView: true);
        }

        private bool CanPartiallyCompleteOnEmptyCompartment()
        {
            this.CanConfirmOnEmpty =
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
                this.InputQuantity.Value >= 0
                &&
                this.InputQuantity.Value < this.MissionOperation.RequestedQuantity;

            return this.CanConfirmOnEmpty;
        }

        private async Task PartiallyCompleteOnEmptyCompartmentAsync()
        {
            this.IsWaitingForResponse = true;
            this.IsOperationConfirmed = true;

            try
            {
                await this.MissionOperationsService.PartiallyCompleteCurrentAsync(this.InputQuantity.Value);
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
