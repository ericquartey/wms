using System;
using System.Linq;
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

        private string withdrawReason;

        #endregion

        #region Properties

        public ICommand ShowLoadingUnitDetailsCommand => this.showLoadingUnitDetailsCommand ??
            (this.showLoadingUnitDetailsCommand = new DelegateCommand(
                    this.ShowLoadingUnitDetails,
                    this.CanShowLoadingUnitDetails)
                .ObservesProperty(() => this.CurrentItem));

        public ICommand WithdrawLoadingUnitCommand => this.withdrawLoadingUnitCommand ??
                (this.withdrawLoadingUnitCommand = new DelegateCommand(WithdrawLoadingUnit));

        public string WithdrawReason
        {
            get => this.withdrawReason;
            set => this.SetProperty(ref this.withdrawReason, value);
        }

        #endregion

        #region Methods

        public override void UpdateMoreReasons()
        {
            this.WithdrawReason = this.CurrentItem?.Policies?.Where(p => p.Name == nameof(BusinessPolicies.Withdraw)).Select(p => p.Reason).FirstOrDefault();
        }

        protected override void ExecuteAddCommand()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.LOADINGUNITADD);
        }

        private static void WithdrawLoadingUnit()
        {
            throw new NotImplementedException();
        }

        private bool CanShowLoadingUnitDetails()
        {
            return this.CurrentItem != null;
        }

        private void ShowLoadingUnitDetails()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.LOADINGUNITDETAILS, this.CurrentItem.Id);
        }

        #endregion
    }
}
