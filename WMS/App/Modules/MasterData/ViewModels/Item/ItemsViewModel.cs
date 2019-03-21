using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemsViewModel : EntityPagedListViewModel<Item, int>
    {
        #region Fields

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private ICommand deleteCommand;

        private ICommand showDetailsCommand;

        private ICommand withdrawCommand;

        #endregion

        #region Properties

        public ICommand DeleteCommand => this.deleteCommand ??
            (this.deleteCommand = new DelegateCommand(
                async () => await this.ExecuteDeleteCommandAsync(),
                this.CanExecuteDeleteCommand).ObservesProperty(() => this.CurrentItem));

        public ICommand ShowDetailsCommand => this.showDetailsCommand ??
            (this.showDetailsCommand = new DelegateCommand(this.ExecuteShowDetailsCommand, this.CanShowDetailsCommand)
                .ObservesProperty(() => this.CurrentItem));

        public ICommand WithdrawCommand => this.withdrawCommand ??
            (this.withdrawCommand = new DelegateCommand(
                this.ExecuteWithdraw,
                this.CanExecuteWithdraw).ObservesProperty(() => this.CurrentItem));

        #endregion

        #region Methods

        protected override void ExecuteAddCommand()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.ITEMADDDIALOG);
        }

        private bool CanExecuteDeleteCommand()
        {
            return this.CurrentItem != null;
        }

        private bool CanExecuteWithdraw()
        {
            return this.CurrentItem?.TotalAvailable > 0;
        }

        private bool CanShowDetailsCommand()
        {
            return this.CurrentItem != null;
        }

        private async Task DeleteItemAsync()
        {
            var userChoice = this.DialogService.ShowMessage(
                string.Format(DesktopApp.AreYouSureToDeleteGeneric, BusinessObjects.ItemListRow),
                DesktopApp.ConfirmOperation,
                DialogType.Question,
                DialogButtons.YesNo);

            if (userChoice == DialogResult.Yes)
            {
                var result = await this.itemProvider.DeleteAsync(this.CurrentItem.Id);
                if (result.Success)
                {
                    this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemDeletedSuccessfully, StatusType.Success));
                    this.SelectedItem = null;
                    this.ExecuteRefreshCommand();
                }
                else
                {
                    this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
                }
            }
        }

        private async Task ExecuteDeleteCommandAsync()
        {
            var deleteAction = await this.itemProvider.CanDeleteAsync(this.CurrentItem.Id);
            if (deleteAction.IsAllowed)
            {
                await this.DeleteItemAsync();
            }
            else
            {
                this.ShowErrorDialog(deleteAction);
            }
        }

        private void ExecuteShowDetailsCommand()
        {
            this.HistoryViewService.Appear(
                nameof(Modules.MasterData),
                Common.Utils.Modules.MasterData.ITEMDETAILS,
                this.CurrentItem.Id);
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

        #endregion
    }
}
