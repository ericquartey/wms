using System;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitsViewModel : EntityListViewModel<LoadingUnit>
    {
        #region Fields

        private ICommand refreshCommand;

        private ICommand showDetailsCommand;

        private ICommand withdrawCommand;

        #endregion Fields

        #region Properties

        public ICommand RefreshCommand => this.refreshCommand ??
                    (this.refreshCommand = new DelegateCommand(
                this.ExecuteRefreshCommand));

        public ICommand ShowDetailsCommand => this.showDetailsCommand ??
                          (this.showDetailsCommand = new DelegateCommand(this.ExecuteShowDetailsCommand, this.CanShowDetailsCommand)
            .ObservesProperty(() => this.CurrentItem));

        public ICommand WithdrawCommand => this.withdrawCommand ??
                (this.withdrawCommand = new DelegateCommand(ExecuteWithdrawCommand));

        #endregion Properties

        #region Methods

        private static void ExecuteWithdrawCommand()
        {
            throw new NotImplementedException();
        }

        private bool CanShowDetailsCommand()
        {
            return this.CurrentItem != null;
        }

        private void ExecuteRefreshCommand()
        {
            this.RefreshData();
        }

        private void ExecuteShowDetailsCommand()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.ITEMLISTDETAILS, this.CurrentItem.Id);
        }

        #endregion Methods
    }
}
