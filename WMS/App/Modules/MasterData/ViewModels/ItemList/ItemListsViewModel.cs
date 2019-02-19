using System;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.Controls;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemListsViewModel : EntityPagedListViewModel<ItemList>
    {
        #region Fields

        private ICommand listExecuteCommand;

        private ICommand showDetailsCommand;

        #endregion

        #region Properties

        public ICommand ListExecuteCommand => this.listExecuteCommand ??
                  (this.listExecuteCommand = new DelegateCommand(
                           this.ExecuteListCommand,
                           this.CanExecuteListCommand)
            .ObservesProperty(() => this.CurrentItem));

        public ICommand ShowDetailsCommand => this.showDetailsCommand ??
                          (this.showDetailsCommand = new DelegateCommand(this.ExecuteShowDetailsCommand, this.CanShowDetailsCommand)
            .ObservesProperty(() => this.CurrentItem));

        #endregion

        #region Methods

        protected override void ExecuteAddCommand()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.ITEMLISTADDDIALOG);
        }

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

        private void ExecuteShowDetailsCommand()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.ITEMLISTDETAILS, this.CurrentItem.Id);
        }

        #endregion
    }
}
