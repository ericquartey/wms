using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces.Models;
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

        private ICommand deleteItemCommand;

        private ICommand showItemDetailsCommand;

        private ICommand withdrawItemCommand;

        #endregion

        #region Properties

        public ICommand DeleteItemCommand => this.deleteItemCommand ??
            (this.deleteItemCommand = new DelegateCommand(
                    async () => await this.DeleteItemAsync(),
                    this.CanDeleteItem)
                .ObservesProperty(() => this.CurrentItem));

        public ICommand ShowItemDetailsCommand => this.showItemDetailsCommand ??
            (this.showItemDetailsCommand = new DelegateCommand(
                    this.ShowItemDetails,
                    this.CanShowItemDetails)
                .ObservesProperty(() => this.CurrentItem));

        public ICommand WithdrawItemCommand => this.withdrawItemCommand ??
            (this.withdrawItemCommand = new DelegateCommand(
                    this.WithdrawItem,
                    this.CanWithdrawItem)
                .ObservesProperty(() => this.CurrentItem));

        #endregion

        #region Methods

        protected override void ExecuteAddCommand()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.ITEMADDDIALOG);
        }

        private bool CanDeleteItem()
        {
            return this.CurrentItem != null;
        }

        private bool CanWithdrawItem()
        {
            return this.CurrentItem != null;
        }

        private bool CanShowItemDetails()
        {
            return this.CurrentItem != null;
        }

        private async Task DeleteItemAsync()
        {
            if (!this.CurrentItem.CanDelete())
            {
                this.ShowErrorDialog(this.CurrentItem.GetCanDeleteReason());
                return;
            }

            var userChoice = this.DialogService.ShowMessage(
                string.Format(DesktopApp.AreYouSureToDeleteGeneric, BusinessObjects.Item),
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

        private void ShowItemDetails()
        {
            this.HistoryViewService.Appear(
                nameof(Modules.MasterData),
                Common.Utils.Modules.MasterData.ITEMDETAILS,
                this.CurrentItem.Id);
        }

        private void WithdrawItem()
        {
            if (!this.CurrentItem.CanExecuteOperation("Withdraw"))
            {
                this.ShowErrorDialog(this.CurrentItem.GetCanExecuteOperationReason("Withdraw"));
                return;
            }

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
