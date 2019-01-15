using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Modules.BLL.Models;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemListRowDetailsViewModel : DetailsViewModel<ItemListRowDetails>
    {
        #region Fields

        private readonly IItemListRowProvider itemListRowProvider = ServiceLocator.Current.GetInstance<IItemListRowProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private IDataSource<Item> itemsDataSource;

        private ICommand listRowExecuteCommand;

        private object modelChangedEventSubscription;

        private object modelRefreshSubscription;

        private object modelSelectionChangedSubscription;

        #endregion Fields

        #region Constructors

        public ItemListRowDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public IDataSource<Item> ItemsDataSource
        {
            get => this.itemsDataSource;
            set => this.SetProperty(ref this.itemsDataSource, value);
        }

        public ICommand ListRowExecuteCommand => this.listRowExecuteCommand ??
                                   (this.listRowExecuteCommand = new DelegateCommand(
                                        this.ExecuteListRowCommand,
                                        this.CanExecuteListRowCommand));

        #endregion Properties

        #region Methods

        protected override void EvaluateCanExecuteCommands()
        {
            base.EvaluateCanExecuteCommands();

            ((DelegateCommand)this.ListRowExecuteCommand)?.RaiseCanExecuteChanged();
        }

        protected override async Task ExecuteRevertCommand()
        {
            await this.LoadDataAsync();
        }

        protected override async Task ExecuteSaveCommand()
        {
            this.IsBusy = true;

            var result = await this.itemListRowProvider.SaveAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedPubSubEvent<Item>(this.Model.Id));
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemListSavedSuccessfully));
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;
        }

        protected override async void OnAppear()
        {
            await this.LoadDataAsync();
            base.OnAppear();
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsPubSubEvent<ItemListRow>>(this.modelRefreshSubscription);
            this.EventService.Unsubscribe<ModelChangedPubSubEvent<ItemListRow>>(this.modelChangedEventSubscription);
            this.EventService.Unsubscribe<ModelSelectionChangedPubSubEvent<ItemListRow>>(
                this.modelSelectionChangedSubscription);
            base.OnDispose();
        }

        private bool CanExecuteListRowCommand()
        {
            return this.Model?.CanBeExecuted == true;
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
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedPubSubEvent<ItemListRow>>(async eventArgs => { await this.LoadDataAsync(); });
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
                this.Model = await this.itemListRowProvider.GetByIdAsync(modelId);

                this.ItemsDataSource = this.Model != null
                ? new DataSource<Item>(() => this.itemProvider.GetAll())
                : null;
            }
        }

        #endregion Methods
    }
}
