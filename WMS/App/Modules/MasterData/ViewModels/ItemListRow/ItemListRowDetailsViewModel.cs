using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using DevExpress.Xpf.Data;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Resources;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemListRowDetailsViewModel : DetailsViewModel<ItemListRowDetails>
    {
        #region Fields

        private readonly IItemListRowProvider itemListRowProvider = ServiceLocator.Current.GetInstance<IItemListRowProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private ICommand deleteCommand;

        private InfiniteAsyncSource itemsDataSource;

        private ICommand listRowExecuteCommand;

        private object modelChangedEventSubscription;

        private object modelRefreshSubscription;

        private object modelSelectionChangedSubscription;

        #endregion

        #region Constructors

        public ItemListRowDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion

        #region Properties

        public ICommand DeleteCommand => this.deleteCommand ??
            (this.deleteCommand = new DelegateCommand(
                async () => await this.ExecuteDeleteCommandAsync()));

        public InfiniteAsyncSource ItemsDataSource
        {
            get => this.itemsDataSource;
            set => this.SetProperty(ref this.itemsDataSource, value);
        }

        public ICommand ListRowExecuteCommand => this.listRowExecuteCommand ??
                                   (this.listRowExecuteCommand = new DelegateCommand(
                                        this.ExecuteListRowCommand,
                                        this.CanExecuteListRowCommand));

        #endregion

        #region Methods

        protected override void EvaluateCanExecuteCommands()
        {
            base.EvaluateCanExecuteCommands();

            ((DelegateCommand)this.ListRowExecuteCommand)?.RaiseCanExecuteChanged();
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

            var result = await this.itemListRowProvider.UpdateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedPubSubEvent<Item, int>(this.Model.Id));
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemListSavedSuccessfully));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;
        }

        protected override async Task OnAppearAsync()
        {
            await this.LoadDataAsync().ConfigureAwait(true);
            await base.OnAppearAsync().ConfigureAwait(true);
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsPubSubEvent<ItemListRow>>(this.modelRefreshSubscription);
            this.EventService.Unsubscribe<ModelChangedPubSubEvent<ItemListRow, int>>(this.modelChangedEventSubscription);
            this.EventService.Unsubscribe<ModelSelectionChangedPubSubEvent<ItemListRow>>(
                this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private bool CanExecuteListRowCommand()
        {
            return this.Model?.CanBeExecuted == true;
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
                var result = await this.itemListRowProvider.DeleteAsync(this.Model.Id);
                if (result.Success)
                {
                    this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemListRowDeletedSuccessfully, StatusType.Success));
                    this.EventService.Invoke(new RefreshModelsPubSubEvent<ItemList>(this.Model.ItemListId));
                    this.HistoryViewService.Previous();
                }
                else
                {
                    this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
                }
            }

            this.IsBusy = false;
        }

        private async Task ExecuteDeleteCommandAsync()
        {
            var deleteAction = await this.itemListRowProvider.CanDeleteAsync(this.Model.Id);
            if (deleteAction.IsAllowed)
            {
                await this.DeleteItemListRowAsync();
            }
            else
            {
                this.ShowErrorDialog(deleteAction);
            }
        }

        private void ExecuteListRowCommand()
        {
            this.IsBusy = true;

            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.EXECUTELISTROWDIALOG,
                new
                {
                    Id = this.Model.Id
                });
            this.IsBusy = false;
        }

        private void Initialize()
        {
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsPubSubEvent<ItemListRow>>(
                async eventArgs => { await this.LoadDataAsync(); },
                this.Token,
                true,
                true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedPubSubEvent<ItemListRow, int>>(async eventArgs => { await this.LoadDataAsync(); });
            this.modelSelectionChangedSubscription =
                this.EventService.Subscribe<ModelSelectionChangedPubSubEvent<ItemListRow>>(
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

                    this.Model = await this.itemListRowProvider.GetByIdAsync(modelId);
                    if (this.Model != null)
                    {
                        this.ItemsDataSource = new InfiniteDataSourceService<Item, int>(this.itemProvider).DataSource;
                    }
                    else
                    {
                        this.ItemsDataSource = null;
                    }

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
