using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemPutViewModel : BaseItemOperationMainViewModel
    {
        #region Fields

        private DelegateCommand fullOperationCommand;

        #endregion

        #region Constructors

        public ItemPutViewModel(
            IWmsImagesProvider wmsImagesProvider,
            IMissionsDataService missionsDataService,
            IMissionOperationsService missionOperationsService,
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            IDialogService dialogService)
            : base(wmsImagesProvider, missionsDataService, bayManager, eventAggregator, missionOperationsService, dialogService)
        {
        }

        #endregion

        #region Properties

        public ICommand FullOperationCommand =>
            this.fullOperationCommand
            ??
            (this.fullOperationCommand = new DelegateCommand(
                async () => await this.PartiallyCompleteOnFullCompartmentAsync(),
                this.CanPartiallyCompleteOnFullCompartmen));

        #endregion

        #region Methods

        public async override Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
        }

        public override void OnMisionOperationRetrieved()
        {
            this.InputQuantity = this.MissionOperation.RequestedQuantity;
        }

        public override void RaiseCanExecuteChanged()
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
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
