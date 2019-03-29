using System;
using System.Windows.Input;
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
                (this.withdrawLoadingUnitCommand = new DelegateCommand(WithdrawLoadingUnit));

        #endregion

        #region Methods

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
