using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Modules.MasterData
{
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Item), false)]
    [Resource(nameof(Ferretto.WMS.Data.WebAPI.Contracts.Compartment), false)]
    public class ItemsViewModel : EntityPagedListViewModel<Item, int>
    {
        #region Fields

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private ICommand pickItemCommand;

        private string pickReason;

        private ICommand putItemCommand;

        private string putReason;

        #endregion

        #region Constructors

        public ItemsViewModel(IDataSourceService dataSourceService)
                    : base(dataSourceService)
        {
        }

        #endregion

        #region Properties

        public ICommand PickItemCommand => this.pickItemCommand ??
            (this.pickItemCommand = new Prism.Commands.DelegateCommand(
                    this.PickItem,
                    this.CanPickItem)
                .ObservesProperty(() => this.CurrentItem));

        public string PickReason
        {
            get => this.pickReason;
            set => this.SetProperty(ref this.pickReason, value);
        }

        public ICommand PutItemCommand => this.putItemCommand ??
                    (this.putItemCommand = new Prism.Commands.DelegateCommand(
                    this.PutItem,
                    this.CanPutItem)
                .ObservesProperty(() => this.CurrentItem));

        public string PutReason
        {
            get => this.putReason;
            set => this.SetProperty(ref this.putReason, value);
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

            this.PickReason = this.CurrentItem?.GetCanExecuteOperationReason(nameof(ItemPolicy.Pick));
            this.PutReason = this.CurrentItem?.GetCanExecuteOperationReason(nameof(ItemPolicy.Put));
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
                this.EventService.Invoke(new StatusPubSubEvent(App.Resources.MasterData.ItemDeletedSuccessfully, StatusType.Success));
                this.SelectedItem = null;
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToSaveChanges, StatusType.Error));
            }
        }

        private bool CanPickItem()
        {
            return this.CurrentItem != null;
        }

        private bool CanPutItem()
        {
            return this.CurrentItem != null;
        }

        private void PickItem()
        {
            if (!this.CurrentItem.CanExecuteOperation(nameof(ItemPolicy.Pick)))
            {
                this.ShowErrorDialog(this.PickReason);
                return;
            }

            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.ITEMPICK,
                new
                {
                    Id = this.CurrentItem.Id,
                });
        }

        private void PutItem()
        {
            if (!this.CurrentItem.CanExecuteOperation(nameof(ItemPolicy.Put)))
            {
                this.ShowErrorDialog(this.PutReason);
                return;
            }

            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.ITEMPUT,
                new
                {
                    Id = this.CurrentItem.Id,
                });
        }

        #endregion
    }
}
