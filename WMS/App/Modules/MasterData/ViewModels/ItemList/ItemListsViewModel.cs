using System;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemListsViewModel : EntityListViewModel<ItemList>
    {
        #region Fields

        private ICommand listExecuteCommand;

        private ICommand refreshCommand;

        private ICommand showDetailsCommand;

        #endregion Fields

        #region Properties

        public ICommand ListExecuteCommand => this.listExecuteCommand ??
                  (this.listExecuteCommand = new DelegateCommand(
                           this.ExecuteListCommand,
                           this.CanExecuteListCommand)
            .ObservesProperty(() => this.CurrentItem));

        public ICommand RefreshCommand => this.refreshCommand ??
                    (this.refreshCommand = new DelegateCommand(
                this.ExecuteRefreshCommand));

        public ICommand ShowDetailsCommand => this.showDetailsCommand ??
                          (this.showDetailsCommand = new DelegateCommand(this.ExecuteShowDetailsCommand, this.CanShowDetailsCommand)
            .ObservesProperty(() => this.CurrentItem));

        #endregion Properties

        #region Methods

        private bool CanExecuteListCommand()
        {
            if (this.CurrentItem != null)
            {
                var status = this.CurrentItem.ItemListStatus;
                if (status == ItemListStatus.Incomplete
                    || status == ItemListStatus.Suspended
                    || status == ItemListStatus.Waiting)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CanShowDetailsCommand()
        {
            return this.CurrentItem != null;
        }

        private void ExecuteListCommand()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.EXECUTELISTDIALOG,
                new
                {
                    Id = this.CurrentItem.Id
                });
        }

        private void ExecuteRefreshCommand()
        {
            this.LoadRelatedData();
        }

        private void ExecuteShowDetailsCommand()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.ITEMLISTDETAILS, this.CurrentItem.Id);
        }

        #endregion Methods
    }
}
