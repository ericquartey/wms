using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemPutViewModel : BaseItemOperationMainViewModel, IOperationalContextViewModel
    {
        #region Fields

        private DelegateCommand fullOperationCommand;

        #endregion

        #region Constructors

        public ItemPutViewModel(
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

        public string ActiveContextName => OperationalContext.ItemPut.ToString();

        public ICommand FullOperationCommand =>
            this.fullOperationCommand
            ??
            (this.fullOperationCommand = new DelegateCommand(
                async () => await this.PartiallyCompleteOnFullCompartmentAsync(),
                this.CanPartiallyCompleteOnFullCompartmen));

        #endregion

        #region Methods

        public Task CommandUserActionAsync(UserActionEventArgs userAction)
        {
            // do nothing
            return Task.CompletedTask;
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.MeasureUnitDescription = String.Format(Resources.Localized.Get("OperatorApp.DrawerActivityRefillingQtyRefilled"), this.MeasureUnit);

            this.RaisePropertyChanged(nameof(this.MeasureUnitDescription));
        }

        public override void OnMisionOperationRetrieved()
        {
            this.InputQuantity = this.MissionOperation.RequestedQuantity - this.MissionOperation.DispatchedQuantity;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.fullOperationCommand.RaiseCanExecuteChanged();
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
                this.InputQuantity.Value < this.MissionOperation.RequestedQuantity;
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
