using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemListsViewModel : EntityPagedListViewModel<ItemList, int>
    {
        #region Fields

        private readonly IItemListProvider itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

        private ICommand executeListCommand;

        private ICommand showListDetailsCommand;

        #endregion

        #region Properties

        public ICommand ExecuteListCommand => this.executeListCommand ??
                    (this.executeListCommand = new DelegateCommand(
                    this.ExecuteList,
                    this.CanExecuteList)
                .ObservesProperty(() => this.CurrentItem));

        public ICommand ShowListDetailsCommand => this.showListDetailsCommand ??
            (this.showListDetailsCommand = new DelegateCommand(
                    this.ShowListDetails,
                    this.CanShowListDetails)
                .ObservesProperty(() => this.CurrentItem));

        #endregion

        #region Methods

        protected override void ExecuteAddCommand()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.ITEMLISTADD);
        }

        protected override async Task ExecuteDeleteCommandAsync()
        {
            var result = await this.itemListProvider.DeleteAsync(this.CurrentItem.Id);
            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemListDeletedSuccessfully, StatusType.Success));
                this.SelectedItem = null;
                this.ExecuteRefreshCommand();
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToSaveChanges, StatusType.Error));
            }
        }

        private bool CanExecuteList()
        {
            return this.CurrentItem?.CanExecuteOperation("Execute") == true;
        }

        private bool CanShowListDetails()
        {
            return this.CurrentItem != null;
        }

        private void ExecuteList()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.EXECUTELISTDIALOG,
                new
                {
                    Id = this.CurrentItem.Id
                });
        }

        private void ShowListDetails()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.ITEMLISTDETAILS, this.CurrentItem.Id);
        }

        #endregion
    }
}
