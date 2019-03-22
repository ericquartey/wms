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

        private ICommand deleteCommand;

        private IEnumerable<ItemListRow> itemListRowDataSource;

        private ICommand listExecuteCommand;

        private bool listHasRows;

        private ICommand listRowExecuteCommand;

        private object modelChangedEventSubscription;

        private object modelRefreshSubscription;

        private object modelSelectionChangedSubscription;

        private ItemListRow selectedItemListRow;

        private ICommand showDetailsListRowCommand;

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
                                        this.ExecuteAddListRowCommand,
                                        this.CanExecuteAddListRowCommand));

        public ICommand DeleteCommand => this.deleteCommand ??
            (this.deleteCommand = new DelegateCommand(
                async () => await this.ExecuteDeleteCommandAsync(),
                this.CanExecuteDeleteCommand).ObservesProperty(() => this.SelectedItemListRow));

        public IEnumerable<ItemListRow> ItemListRowDataSource
        {
            get => this.itemListRowDataSource;
            set => this.SetProperty(ref this.itemListRowDataSource, value);
        }

        public ICommand ListExecuteCommand => this.listExecuteCommand ??
                                   (this.listExecuteCommand = new DelegateCommand(
                                        this.ExecuteListCommand,
                                        this.CanExecuteListCommand));

        public bool ListHasRows
        {
            get => this.listHasRows;
            set => this.SetProperty(ref this.listHasRows, value);
        }

        public ICommand ListRowExecuteCommand => this.listRowExecuteCommand ??
                                   (this.listRowExecuteCommand = new DelegateCommand(
                                        this.ExecuteListRowCommand,
                                        this.CanExecuteListRowCommand).ObservesProperty(() => this.SelectedItemListRow));

        public ItemListRow SelectedItemListRow
        {
            get => this.selectedItemListRow;
            set => this.SetProperty(ref this.selectedItemListRow, value);
        }

        public ICommand ShowDetailsListRowCommand => this.showDetailsListRowCommand ??
                  (this.showDetailsListRowCommand = new DelegateCommand(
                       this.ExecuteShowDetailsListRowCommand,
                       this.CanExecuteShowDetailsListRowCommand).ObservesProperty(() => this.SelectedItemListRow));

        #endregion

        #region Methods

        public override void LoadRelatedData()
        {
            // TODO: implement method
        }

        protected override void EvaluateCanExecuteCommands()
        {
            base.EvaluateCanExecuteCommands();
            ((DelegateCommand)this.ListExecuteCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.ListRowExecuteCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.ShowDetailsListRowCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.AddListRowCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.DeleteCommand)?.RaiseCanExecuteChanged();
        }

        protected override async Task ExecuteRefreshCommandAsync()
        {
            await this.LoadDataAsync();
        }

        protected override async Task ExecuteRevertCommand()
        {
            await this.LoadDataAsync();
        }

        protected override async Task ExecuteSaveCommand()
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

        private bool CanExecuteAddListRowCommand()
        {
            return this.Model?.CanAddNewRow == true;
        }

        private bool CanExecuteDeleteCommand()
        {
            return this.selectedItemListRow != null;
        }

        private bool CanExecuteListCommand()
        {
            return this.Model?.CanBeExecuted == true;
        }

        private bool CanExecuteListRowCommand()
        {
            return this.selectedItemListRow?.CanBeExecuted == true;
        }

        private bool CanExecuteShowDetailsListRowCommand()
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

        private void ExecuteAddListRowCommand()
        {
            this.IsBusy = true;

            // TODO
            this.IsBusy = false;
        }

        private async Task ExecuteDeleteCommandAsync()
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

        private void ExecuteListCommand()
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

        private void ExecuteListRowCommand()
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

        private void ExecuteShowDetailsListRowCommand()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.ITEMLISTROWDETAILS, this.SelectedItemListRow.Id);
        }

        private void Initialize()
        {
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsPubSubEvent<ItemList>>(
                async eventArgs => { await this.LoadDataAsync(); },
                this.Token,
                keepSubscriberReferenceAlive: true,
                forceUiThread: true);

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
                    keepSubscriberReferenceAlive: true,
                    forceUiThread: true);
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
                    this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToLoadData, StatusType.Error));
                }
            }
        }

        #endregion
    }
}
