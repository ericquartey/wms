using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemsViewModel : EntityPagedListViewModel<Item, int>
    {
        #region Fields

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private ICommand withdrawItemCommand;

        private string withdrawReason;

        #endregion

        #region Constructors

        public ItemsViewModel(IDataSourceService dataSourceService)
                                  : base(dataSourceService)
        {
        }

        #endregion

        #region Properties

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

        public override void ShowDetails()
        {
            this.HistoryViewService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.ITEMDETAILS,
                this.CurrentItem.Id);
        }

        public override void UpdateReasons()
        {
            base.UpdateReasons();
            this.WithdrawReason = this.CurrentItem?.Policies?.Where(p => p.Name == nameof(BusinessPolicies.Withdraw)).Select(p => p.Reason).FirstOrDefault();
        }

        protected override void ExecuteAddCommand()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.ITEMADD);
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

        private bool CanWithdrawItem()
        {
            return this.CurrentItem != null;
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
                Common.Utils.Modules.MasterData.ITEMWITHDRAW,
                new
                {
                    Id = this.CurrentItem.Id
                });
        }

        #endregion
    }
}
