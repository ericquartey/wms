using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemListDetailsViewModel : DetailsViewModel<ItemListDetails>
    {
        #region Fields

        private readonly IItemListProvider itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

        private ICommand addListRowCommand;

        private IEnumerable<ItemListRow> itemListRowDataSource;

        private ICommand listExecuteCommand;

        private ICommand listRowExecuteCommand;

        private object modelChangedEventSubscription;

        private object modelRefreshSubscription;

        private object modelSelectionChangedSubscription;

        private ItemListRow selectedItemListRow;

        private ICommand showDetailsListRowCommand;
        private bool listHasRows;

        #endregion Fields

        #region Constructors

        public ItemListDetailsViewModel()
        {
            this.Initialize();
        }

        #endregion Constructors

        #region Properties

        public bool ListHasRows {
            get => this.listHasRows;
            set => this.SetProperty(ref this.listHasRows, value); }

        public ICommand AddListRowCommand => this.addListRowCommand ??
                                   (this.addListRowCommand = new DelegateCommand(this.ExecuteAddListRowCommand,
                       this.CanExecuteAddListRowCommand));

        public IEnumerable<ItemListRow> ItemListRowDataSource
        {
            get => this.itemListRowDataSource;
            set => this.SetProperty(ref this.itemListRowDataSource, value);
        }

        public ICommand ListExecuteCommand => this.listExecuteCommand ??
                                   (this.listExecuteCommand = new DelegateCommand(this.ExecuteListCommand,
                       this.CanExecuteListCommand));

        public ICommand ListRowExecuteCommand => this.listRowExecuteCommand ??
                                   (this.listRowExecuteCommand = new DelegateCommand(this.ExecuteListRowCommand,
                       this.CanExecuteListRowCommand));

        public ItemListRow SelectedItemListRow
        {
            get => this.selectedItemListRow;
            set => this.SetProperty(ref this.selectedItemListRow, value);
        }

        public ICommand ShowDetailsListRowCommand => this.showDetailsListRowCommand ??
                  (this.showDetailsListRowCommand = new DelegateCommand(this.ExecuteShowDetailsListRowCommand,
                       this.CanExecuteShowDetailsListRowCommand));

        #endregion Properties

        #region Methods
        protected override void EvaluateCanExecuteCommands()
        {
            base.EvaluateCanExecuteCommands();
            ((DelegateCommand)this.ListExecuteCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.ListRowExecuteCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.ShowDetailsListRowCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommand)this.AddListRowCommand)?.RaiseCanExecuteChanged();
        }

        public override void RefreshData()
        {
            // TODO: implement method
        }

        protected override async Task ExecuteRevertCommand()
        {
            await this.LoadData();
        }

        protected override void ExecuteSaveCommand()
        {
            this.IsBusy = true;

            var modifiedRowCount = this.itemListProvider.Save(this.Model);
            if (modifiedRowCount > 0)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedEvent<Item>(this.Model.Id));
                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.ItemListSavedSuccessfully));
            }
            this.IsBusy = false;
        }

        protected override async void OnAppear()
        {
            base.OnAppear();
            await this.LoadData();
        }

        private bool CanExecuteAddListRowCommand()
        {
            return this.Model?.CanAddNewRow == true;
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

        private void ExecuteAddListRowCommand()
        {
            this.IsBusy = true;

            //TODO

            this.IsBusy = false;
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
                }
            );

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
                }
            );

            this.IsBusy = false;
        }

        private void ExecuteShowDetailsListRowCommand()
        {
            this.HistoryViewService.Appear(nameof(Modules.MasterData), Common.Utils.Modules.MasterData.ITEMLISTROWDETAILS, this.SelectedItemListRow.Id);
        }

        private async void Initialize()
        {
            await this.LoadData();

            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsEvent<ItemList>>(async eventArgs => { await this.LoadData(); }, this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedEvent<ItemList>>(async eventArgs => { await this.LoadData(); });
            this.modelSelectionChangedSubscription =
                this.EventService.Subscribe<ModelSelectionChangedEvent<ItemList>>(
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
            this.IsBusy = true;

            if ((this.Data is int modelId))
            {
                this.Model = await this.itemListProvider.GetById(modelId);
                this.ListHasRows = this.Model.ItemListRowsCount > 0;
            }

            this.IsBusy = false;
        }

        #endregion Methods
    }
}
