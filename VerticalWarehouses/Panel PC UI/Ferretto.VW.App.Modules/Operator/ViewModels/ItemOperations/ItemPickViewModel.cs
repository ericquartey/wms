using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ItemPickViewModel : BaseItemOperationMainViewModel
    {
        #region Fields

        private DelegateCommand emptyOperationCommand;

        #endregion

        #region Constructors

        public ItemPickViewModel(
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

        public ICommand EmptyOperationCommand =>
            this.emptyOperationCommand
            ??
            (this.emptyOperationCommand = new DelegateCommand(
                async () => await this.PartiallyCompleteOnEmptyCompartmentAsync(),
                this.CanPartiallyCompleteOnEmptyCompartment));

        public override EnableMask EnableMask => EnableMask.Any;

        #endregion

        #region Methods
        public override Task OnAppearedAsync()
        {
            this.Compartments = null;
            this.SelectedCompartment = null;
            return base.OnAppearedAsync();
        }

        public override void OnMisionOperationRetrieved()
        {
            this.InputQuantity = this.MissionOperation.RequestedQuantity;
        }

        public override void RaiseCanExecuteChanged()
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

        private async Task PartiallyCompleteOnEmptyCompartmentAsync()
        {
            await this.MissionOperationsService.PartiallyCompleteCurrentAsync(this.InputQuantity.Value);
        }

        #endregion
    }
}
