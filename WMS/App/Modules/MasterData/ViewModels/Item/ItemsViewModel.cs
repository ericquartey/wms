using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemsViewModel : EntityPagedListViewModel<Item>
    {
        #region Fields

        private ICommand showFiltersCommand;

        private ICommand showDetailsCommand;

        private ICommand withdrawCommand;

        #endregion Fields

        #region Properties

        public ICommand ShowFiltersCommand => this.showFiltersCommand ??
                       (this.showFiltersCommand = new DelegateCommand(this.ExecuteShowFiltersCommand));

        public ICommand ShowDetailsCommand => this.showDetailsCommand ??
                                 (this.showDetailsCommand = new DelegateCommand(this.ExecuteShowDetailsCommand, this.CanShowDetailsCommand)
           .ObservesProperty(() => this.CurrentItem));

        public ICommand WithdrawCommand => this.withdrawCommand ??
                                                   (this.withdrawCommand = new DelegateCommand(
                                                        this.ExecuteWithdraw,
                                                        this.CanExecuteWithdraw).ObservesProperty(() => this.CurrentItem));

        #endregion Properties

        #region Methods

        private bool CanExecuteWithdraw()
        {
            return this.CurrentItem?.TotalAvailable > 0;
        }

        private bool CanShowDetailsCommand()
        {
            return this.CurrentItem != null;
        }

        private void ExecuteShowFiltersCommand()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.FILTERDIALOG,
                this.FilterContext);
        }

        private void ExecuteShowDetailsCommand()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.ITEMDETAILS, this.CurrentItem.Id);
        }

        private void ExecuteWithdraw()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.WITHDRAWDIALOG,
                new
                {
                    Id = this.CurrentItem.Id
                });
        }

        #endregion Methods
    }
}
