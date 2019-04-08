using System.Linq;
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

        private string withdrawReason;

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

        public string WithdrawReason
        {
            get => this.withdrawReason;
            set => this.SetProperty(ref this.withdrawReason, value);
        }

        #endregion

        #region Methods

        public override void UpdateReasons()
        {
            base.UpdateReasons();
            this.WithdrawReason = this.CurrentItem?.Policies?.Where(p => p.Name == nameof(BusinessPolicies.Withdraw)).Select(p => p.Reason).FirstOrDefault();
        }

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

        private bool CanShowItemDetails()
        {
            return this.CurrentItem != null;
        }

        private bool CanWithdrawItem()
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
                nameof(MasterData),
                Common.Utils.Modules.MasterData.ITEMDETAILS,
                this.CurrentItem.Id);
        }

        private void WithdrawItem()
        {
            if (!this.CurrentItem.CanExecuteOperation(nameof(BusinessPolicies.Withdraw)))
            {
                this.ShowErrorDialog(this.CurrentItem.GetCanExecuteOperationReason(nameof(BusinessPolicies.Withdraw)));
                return;
            }

            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.ITEMWITHDRAWDIALOG,
                new
                {
                    Id = this.CurrentItem.Id
                });
        }

        #endregion
    }
}
