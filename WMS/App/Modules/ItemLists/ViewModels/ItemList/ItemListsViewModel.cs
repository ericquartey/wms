using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.ItemLists
{
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.ItemList), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.ItemListRow), false)]
    public class ItemListsViewModel : EntityPagedListViewModel<ItemList, int>
    {
        #region Fields

        private readonly IItemListProvider itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

        private ICommand executeListCommand;

        private string executeReason;

        #endregion

        #region Constructors

        public ItemListsViewModel(IDataSourceService dataSourceService)
                                  : base(dataSourceService)
        {
        }

        #endregion

        #region Properties

        public ICommand ExecuteListCommand => this.executeListCommand ??
                    (this.executeListCommand = new DelegateCommand(
                    this.ExecuteList,
                    this.CanExecuteList)
                .ObservesProperty(() => this.CurrentItem));

        public string ExecuteReason
        {
            get => this.executeReason;
            set => this.SetProperty(ref this.executeReason, value);
        }

        #endregion

        #region Methods

        public override void ShowDetails()
        {
            this.HistoryViewService.Appear(nameof(Modules.ItemLists), Common.Utils.Modules.ItemLists.ITEMLISTDETAILS, this.CurrentItem.Id);
        }

        public override void UpdateReasons()
        {
            base.UpdateReasons();
            this.ExecuteReason = this.CurrentItem?.GetCanExecuteOperationReason(nameof(ItemListPolicy.Execute));
        }

        protected override void ExecuteAddCommand()
        {
            this.NavigationService.Appear(
                nameof(Common.Utils.Modules.ItemLists),
                Common.Utils.Modules.ItemLists.ITEMLISTADD);
        }

        protected override async Task ExecuteDeleteCommandAsync()
        {
            var result = await this.itemListProvider.DeleteAsync(this.CurrentItem.Id);
            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.ItemLists.ItemListDeletedSuccessfully, StatusType.Success));
                this.SelectedItem = null;
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToSaveChanges, StatusType.Error));
            }
        }

        private bool CanExecuteList()
        {
            return this.CurrentItem != null;
        }

        private void ExecuteList()
        {
            if (this.CurrentItem?.CanExecuteOperation(nameof(ItemListPolicy.Execute)) == true)
            {
                this.NavigationService.Appear(
                    nameof(Common.Utils.Modules.ItemLists),
                    Common.Utils.Modules.ItemLists.EXECUTELIST,
                    new
                    {
                        Id = this.CurrentItem.Id,
                    });
            }
            else
            {
                this.ShowErrorDialog(this.ExecuteReason);
            }
        }

        #endregion
    }
}
