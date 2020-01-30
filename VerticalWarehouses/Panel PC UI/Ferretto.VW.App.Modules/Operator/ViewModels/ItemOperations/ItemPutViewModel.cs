using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemPutViewModel : BaseItemOperationMainViewModel, IOperationalContextViewModel
    {
        #region Fields

        private DelegateCommand fullOperationCommand;

        #endregion

        #region Constructors

        public ItemPutViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IItemsWmsWebService itemsWmsWebService,
            IMissionsWmsWebService missionsWmsWebService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            IDialogService dialogService)
            : base(wmsImagesProvider, missionsWmsWebService, itemsWmsWebService, bayManager, eventAggregator, missionOperationsService, dialogService)
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

        public async Task CommandUserActionAsync(UserActionEventArgs userAction)
        {
        }

        public override void OnMisionOperationRetrieved()
        {
            this.InputQuantity = this.MissionOperation.RequestedQuantity;
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
               null,
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
            try
            {
                await this.MissionOperationsService.PartiallyCompleteCurrentAsync(this.InputQuantity.Value);
            }
            catch (MasWebApiException ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
