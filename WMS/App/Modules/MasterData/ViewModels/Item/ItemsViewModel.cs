using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemsViewModel : EntityPagedListViewModel<Item, int>
    {
        #region Fields

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private ICommand showItemDetailsCommand;

        private ICommand withdrawItemCommand;

        #endregion

        #region Properties

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

        protected override async Task ExecuteDeleteCommandAsync()
        {
            var result = await this.itemProvider.DeleteAsync(this.CurrentItem.Id);
            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemDeletedSuccessfully, StatusType.Success));
                this.SelectedItem = null;
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }
        }

        private bool CanDeleteItem()
        {
            return this.CurrentItem != null;
        }

        private bool CanShowItemDetails()
        {
            return this.CurrentItem != null;
        }

        private bool CanWithdrawItem()
        {
            return this.CurrentItem != null;
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
