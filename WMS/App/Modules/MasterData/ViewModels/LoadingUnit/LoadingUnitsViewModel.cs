using System;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitsViewModel : EntityPagedListViewModel<LoadingUnit, int>
    {
        #region Fields

        private ICommand showLoadingUnitDetailsCommand;

        private ICommand withdrawLoadingUnitCommand;

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

        #endregion

        #region Methods

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
                    this.CurrentItem.Id);
            }
        }

        #endregion
    }
}
