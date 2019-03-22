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

        private ICommand showDetailsCommand;

        private ICommand withdrawCommand;

        #endregion

        #region Properties

        public ICommand ShowDetailsCommand => this.showDetailsCommand ??
                          (this.showDetailsCommand = new DelegateCommand(this.ExecuteShowDetailsCommand, this.CanShowDetailsCommand)
            .ObservesProperty(() => this.CurrentItem));

        public ICommand WithdrawCommand => this.withdrawCommand ??
                (this.withdrawCommand = new DelegateCommand(ExecuteWithdrawCommand));

        #endregion

        #region Methods

        protected override void ExecuteAddCommand()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.LOADINGUNITADD);
        }

        private static void ExecuteWithdrawCommand()
        {
            throw new NotImplementedException();
        }

        private bool CanShowDetailsCommand()
        {
            return this.CurrentItem != null;
        }

        private void ExecuteShowDetailsCommand()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.ITEMLISTDETAILS, this.CurrentItem.Id);
        }

        #endregion
    }
}
