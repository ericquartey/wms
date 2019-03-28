using System.Collections.Generic;
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
    public class ItemListDetailsViewModel : DetailsViewModel<ItemListDetails>
    {
        #region Fields

        private readonly IItemListProvider itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

        private readonly IItemListRowProvider itemListRowProvider = ServiceLocator.Current.GetInstance<IItemListRowProvider>();

        private ICommand addListRowCommand;

        private ICommand deleteListRowCommand;

        private IEnumerable<ItemListRow> itemListRowDataSource;

        private ICommand executeListCommand;

        private bool listHasRows;

        private ICommand executeListRowCommand;

        private object modelChangedEventSubscription;

        private object modelRefreshSubscription;

        private object modelSelectionChangedSubscription;

        private ItemListRow selectedItemListRow;

        private ICommand showListRowDetailsCommand;

        #endregion

        #region Constructors

        public ItemListDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public ICommand AddListRowCommand => this.addListRowCommand ??
            (this.addListRowCommand = new DelegateCommand(
                this.AddListRow,
                this.CanAddListRow));

        public ICommand DeleteListRowCommand => this.deleteListRowCommand ??
            (this.deleteListRowCommand = new DelegateCommand(
                async () => await this.DeleteListRowAsync(),
                this.CanDeleteListRow).ObservesProperty(() => this.SelectedItemListRow));

        public IEnumerable<ItemListRow> ItemListRowDataSource
        {
            get => this.itemListRowDataSource;
            set => this.SetProperty(ref this.itemListRowDataSource, value);
        }

        public ICommand ExecuteListCommand => this.executeListCommand ??
            (this.executeListCommand = new DelegateCommand(
                this.ExecuteList,
                this.CanExecuteList));

        public bool ListHasRows
        {
            get => this.listHasRows;
            set => this.SetProperty(ref this.listHasRows, value);
        }

        public ICommand ExecuteListRowCommand => this.executeListRowCommand ??
            (this.executeListRowCommand = new DelegateCommand(
                    this.ExecuteListRow,
                    this.CanExecuteListRow)
                .ObservesProperty(() => this.SelectedItemListRow));

        public ItemListRow SelectedItemListRow
        {
            get => this.selectedItemListRow;
            set => this.SetProperty(ref this.selectedItemListRow, value);
        }

        public ICommand ShowListRowDetailsCommand => this.showListRowDetailsCommand ??
                  (this.showListRowDetailsCommand = new DelegateCommand(
                       this.ShowListRowDetails,
                       this.CanShowListRowDetails).ObservesProperty(() => this.SelectedItemListRow));

        #endregion

        #region Methods

        public override void LoadRelatedData()
        {
            // TODO: implement method
        }

        protected override void EvaluateCanExecuteCommands()
        {
            base.EvaluateCanExecuteCommands();
            ((DelegateCommand)this.ExecuteListCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.ExecuteListRowCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.ShowListRowDetailsCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.AddListRowCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.DeleteListRowCommand)?.RaiseCanExecuteChanged();
        }

        protected override async Task ExecuteRefreshCommandAsync()
        {
            await this.LoadDataAsync();
        }

        protected override async Task ExecuteRevertCommandAsync()
        {
            await this.LoadDataAsync();
        }

        protected override async Task ExecuteSaveCommandAsync()
        {
            this.IsBusy = true;

            var result = await this.itemListProvider.UpdateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedPubSubEvent<Item, int>(this.Model.Id));
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemListSavedSuccessfully, StatusType.Success));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;
        }

        protected override async Task OnAppearAsync()
        {
            await base.OnAppearAsync().ConfigureAwait(true);
            await this.LoadDataAsync().ConfigureAwait(true);
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsPubSubEvent<ItemList>>(this.modelRefreshSubscription);
            this.EventService.Unsubscribe<ModelChangedPubSubEvent<ItemList, int>>(this.modelChangedEventSubscription);
            this.EventService.Unsubscribe<ModelSelectionChangedPubSubEvent<ItemList>>(
                this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private bool CanAddListRow()
        {
            return this.Model?.CanExecuteOperation("AddRow") == true;
        }

        private bool CanDeleteListRow()
        {
            return this.selectedItemListRow?.CanDelete() == true;
        }

        private bool CanExecuteList()
        {
            return this.Model?.CanExecuteOperation("Execute") == true;
        }

        private bool CanExecuteListRow()
        {
            return this.selectedItemListRow?.CanExecuteOperation("Execute") == true;
        }

        private bool CanShowListRowDetails()
        {
            return this.selectedItemListRow != null;
        }

        private async Task DeleteItemListRowAsync()
        {
            this.IsBusy = true;

            var userChoice = this.DialogService.ShowMessage(
                string.Format(DesktopApp.AreYouSureToDeleteGeneric, BusinessObjects.ItemListRow),
                DesktopApp.ConfirmOperation,
                DialogType.Question,
                DialogButtons.YesNo);

            if (userChoice == DialogResult.Yes)
            {
                var result = await this.itemListRowProvider.DeleteAsync(this.SelectedItemListRow.Id);
                if (result.Success)
                {
                    this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemListRowDeletedSuccessfully, StatusType.Success));
                    this.IsBusy = false;
                    this.SelectedItemListRow = null;

                    await this.LoadDataAsync();
                }
                else
                {
                    this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
                }
            }

            this.IsBusy = false;
        }

        private void AddListRow()
        {
            this.IsBusy = true;

            this.NavigationService.Appear(
                            nameof(MasterData),
                            Common.Utils.Modules.MasterData.ITEMLISTROWADD,
                            this.Model.Id);

            this.IsBusy = false;
        }

        private async Task DeleteListRowAsync()
        {
            if (this.SelectedItemListRow.CanDelete())
            {
                await this.DeleteItemListRowAsync();
            }
            else
            {
                this.ShowErrorDialog(this.SelectedItemListRow.GetCanDeleteReason());
            }
        }

        private void ExecuteList()
        {
            this.IsBusy = true;

            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.EXECUTELISTDIALOG,
                new
                {
                    Id = this.Model.Id
                });

            this.IsBusy = false;
        }

        private void ExecuteListRow()
        {
            this.IsBusy = true;

            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.EXECUTELISTROWDIALOG,
                new
                {
                    Id = this.SelectedItemListRow.Id
                });

            this.IsBusy = false;
        }

        private void ShowListRowDetails()
        {
            this.HistoryViewService.Appear(
                nameof(Modules.MasterData),
                Common.Utils.Modules.MasterData.ITEMLISTROWDETAILS,
                this.SelectedItemListRow.Id);
        }

        private void Initialize()
        {
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsPubSubEvent<ItemList>>(
                async eventArgs => { await this.LoadDataAsync(); },
                this.Token,
                true,
                true);

            this.modelChangedEventSubscription = this.EventService
                .Subscribe<ModelChangedPubSubEvent<ItemList, int>>(
                    async eventArgs => { await this.LoadDataAsync(); });

            this.modelSelectionChangedSubscription = this.EventService
                .Subscribe<ModelSelectionChangedPubSubEvent<ItemList>>(
                    async eventArgs =>
                    {
                        if (eventArgs.ModelId.HasValue)
                        {
                            this.Data = eventArgs.ModelId.Value;
                            await this.LoadDataAsync();
                        }
                        else
                        {
                            this.Model = null;
                        }
                    },
                    this.Token,
                    true,
                    true);
        }

        private async Task LoadDataAsync()
        {
            if (this.Data is int modelId)
            {
                try
                {
                    this.IsBusy = true;

                    this.Model = await this.itemListProvider.GetByIdAsync(modelId);
                    this.ListHasRows = this.Model.ItemListRowsCount > 0;

                    this.IsBusy = false;
                }
                catch
                {
                    this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToLoadData, StatusType.Error));
                }
            }
        }

        #endregion
    }
}
