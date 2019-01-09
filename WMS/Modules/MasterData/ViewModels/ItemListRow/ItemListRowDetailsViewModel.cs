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
                                   (this.listRowExecuteCommand = new DelegateCommand(this.ExecuteListRowCommand,
                       this.CanExecuteListRowCommand));

        #endregion Properties

        #region Methods

        protected override async Task ExecuteRevertCommand()
        {
            await this.LoadData();
        }
        protected override void EvaluateCanExecuteCommands()
        {
            base.EvaluateCanExecuteCommands();

            ((DelegateCommand)this.ListRowExecuteCommand)?.RaiseCanExecuteChanged();
        }

        protected override void ExecuteSaveCommand()
        {
            this.IsBusy = true;

            var modifiedRowCount = this.itemListRowProvider.Save(this.Model);
            if (modifiedRowCount > 0)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedPubSubEvent<ItemListRow>(this.Model.Id));
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemListRowSavedSuccessfully));
            }
            this.IsBusy = false;
        }

        protected override async void OnAppear()
        {
            await this.LoadData();
            base.OnAppear();
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
                }
            );
            this.IsBusy = false;
        }

        private async void Initialize()
        {
            await this.LoadData();

            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsPubSubEvent<ItemListRow>>(async eventArgs => { await this.LoadData(); }, this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedPubSubEvent<ItemListRow>>(async eventArgs => { await this.LoadData(); });
            this.modelSelectionChangedSubscription =
                this.EventService.Subscribe<ModelSelectionChangedPubSubEvent<ItemListRow>>(
                    async eventArgs =>
                    {
                        if (eventArgs.ModelId.HasValue)
                        {
                            this.Data = eventArgs.ModelId.Value;
                            await this.LoadData();
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

        private async Task LoadData()
        {
            if (this.Data is int modelId)
            {
                this.Model = await this.itemListRowProvider.GetById(modelId);

                this.ItemsDataSource = this.Model != null
                ? new DataSource<Item>(() => this.itemProvider.GetAll())
                : null;
            }
        }

        #endregion Methods
    }
}
