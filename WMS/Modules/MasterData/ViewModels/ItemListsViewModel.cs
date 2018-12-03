using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemListsViewModel : EntityListViewModel<ItemList>
    {
        #region Fields

        private ICommand editCommand;
        private ICommand listExecuteCommand;

        #endregion Fields

        #region Properties

        public ICommand EditCommand => this.editCommand ??
                  (this.editCommand = new DelegateCommand(this.ExecuteEditCommand));

        public ICommand ListExecuteCommand => this.listExecuteCommand ??
                  (this.listExecuteCommand = new DelegateCommand(this.ExecuteListCommand,
                      this.CanExecuteListCommand)
            .ObservesProperty(() => this.CurrentItem));

        #endregion Properties

        #region Methods

        private bool CanExecuteListCommand()
        {
            if (this.CurrentItem != null)
            {
                var status = this.CurrentItem.ItemListStatusDescription;
                if (status == ItemListStatus.Incomplete.ToString()
                    || status == ItemListStatus.Suspended.ToString()
                    || status == ItemListStatus.Waiting.ToString())
                {
                    return true;
                }
            }
            return false;
        }

        private void ExecuteEditCommand()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.ITEMLISTDETAILS, this.CurrentItem.Id);
        }

        private void ExecuteListCommand()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.EXECUTELISTDIALOG,
                new
                {
                    Id = this.CurrentItem.Id
                }
            );
        }

        #endregion Methods
    }
}
