using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitsViewModel : EntityPagedListViewModel<LoadingUnit, int>
    {
        #region Fields

        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

        private ICommand showLoadingUnitDetailsCommand;

        private ICommand withdrawLoadingUnitCommand;

        private string withdrawReason;

        #endregion

        #region Properties

        public ICommand ShowLoadingUnitDetailsCommand => this.showLoadingUnitDetailsCommand ??
            (this.showLoadingUnitDetailsCommand = new DelegateCommand(
                    this.ShowLoadingUnitDetails,
                    this.CanShowLoadingUnitDetails)
                .ObservesProperty(() => this.CurrentItem));

        public ICommand WithdrawLoadingUnitCommand => this.withdrawLoadingUnitCommand ??
                (this.withdrawLoadingUnitCommand = new DelegateCommand(
                    this.WithdrawLoadingUnit,
                    this.CanWithdrawLoadingUnit));

        public string WithdrawReason
        {
            get => this.withdrawReason;
            set => this.SetProperty(ref this.withdrawReason, value);
        }

        #endregion

        #region Methods

        public override void UpdateReasons()
        {
            base.UpdateReasons();
            this.WithdrawReason = this.CurrentItem?.Policies?.Where(p => p.Name == nameof(BusinessPolicies.Withdraw)).Select(p => p.Reason).FirstOrDefault();
        }

        protected override void EvaluateCanExecuteCommands()
        {
            base.EvaluateCanExecuteCommands();
            ((DelegateCommand)this.WithdrawLoadingUnitCommand)?.RaiseCanExecuteChanged();
        }

        protected override void ExecuteAddCommand()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.LOADINGUNITADD);
        }

        protected override async Task ExecuteDeleteCommandAsync()
        {
            var result = await this.loadingUnitProvider.DeleteAsync(this.CurrentItem.Id);
            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.LoadingUnitDeletedSuccessfully, StatusType.Success));
                this.SelectedItem = null;
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }
        }

        private bool CanShowLoadingUnitDetails()
        {
            return this.CurrentItem != null;
        }

        private bool CanWithdrawLoadingUnit()
        {
            return this.SelectedItem != null;
        }

        private void ShowLoadingUnitDetails()
        {
            this.HistoryViewService.Appear(nameof(MasterData), Common.Utils.Modules.MasterData.LOADINGUNITDETAILS, this.CurrentItem.Id);
        }

        private void WithdrawLoadingUnit()
        {
            if (this.CurrentItem is IPolicyDescriptor<IPolicy> selectedItem)
            {
                if (!selectedItem.CanExecuteOperation("Withdraw"))
                {
                    this.ShowErrorDialog(selectedItem.GetCanExecuteOperationReason("Withdraw"));
                    return;
                }

                this.NavigationService.Appear(
                    nameof(MasterData),
                    Common.Utils.Modules.MasterData.LOADINGUNITWITHDRAW,
                    new
                    {
                        LoadingUnitId = this.CurrentItem.Id
                    });
            }
        }

        #endregion
    }
}
