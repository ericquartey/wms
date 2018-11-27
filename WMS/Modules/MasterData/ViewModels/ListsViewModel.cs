using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ListsViewModel : EntityListViewModel<ItemList>
    {
        #region Fields

        private ICommand withdrawCommand;

        #endregion Fields

        #region Properties

        public ICommand WithdrawCommand => this.withdrawCommand ??
                                           (this.withdrawCommand = new DelegateCommand(this.ExecuteWithdraw,
                                               this.CanExecuteWithdraw).ObservesProperty(() => this.CurrentItem));

        #endregion Properties

        #region Methods

        private bool CanExecuteWithdraw()
        {
            return this.CurrentItem?.TotalAvailable > 0;
        }

        private void ExecuteWithdraw()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.WITHDRAWDIALOG,
                new
                {
                    Id = this.CurrentItem.Id
                }
            );
        }

        #endregion Methods
    }
}
