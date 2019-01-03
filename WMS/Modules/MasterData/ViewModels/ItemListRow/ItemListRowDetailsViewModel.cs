using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private ItemListRowDetails itemListRow;

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

        public ItemListRowDetails ItemListRow
        {
            get => this.itemListRow;
            set => this.SetProperty(ref this.itemListRow, value);
        }

        public IDataSource<Item> ItemsDataSource
        {
            get => this.itemsDataSource;
            set => this.SetProperty(ref this.itemsDataSource, value);
        }

        public ICommand ListRowExecuteCommand => this.listRowExecuteCommand ??
                                   (this.listRowExecuteCommand = new DelegateCommand(this.ExecuteListRowCommand,
                       this.CanExecuteListRowCommand)
             .ObservesProperty(() => this.ItemListRow));

        #endregion Properties

        #region Methods

        protected override async Task ExecuteRevertCommand()
        {
            await this.LoadData();
        }

        protected override void ExecuteSaveCommand()
        {
            var modifiedRowCount = this.itemListRowProvider.Save(this.itemListRow);
            if (modifiedRowCount > 0)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedEvent<ItemListRow>(this.itemListRow.Id));
                this.EventService.Invoke(new StatusEventArgs(Common.Resources.MasterData.ItemListRowSavedSuccessfully));
            }
        }

        protected override async void OnAppear()
        {
            await this.LoadData();
            base.OnAppear();
        }

        private bool CanExecuteListRowCommand()
        {
            if (this.ItemListRow != null)
            {
                var status = this.ItemListRow.ItemListRowStatus;
                if (status == ItemListRowStatus.Incomplete
                    || status == ItemListRowStatus.Suspended
                    || status == ItemListRowStatus.Waiting)
                {
                    return true;
                }
            }
            return false;
        }

        private void ExecuteListRowCommand()
        {
            this.NavigationService.Appear(
                nameof(MasterData),
                Common.Utils.Modules.MasterData.EXECUTELISTROWDIALOG,
                new
                {
                    Id = this.ItemListRow.Id
                }
            );
        }

        private async Task Initialize()
        {
            await this.LoadData();

            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsEvent<ItemListRow>>(async eventArgs => { await this.LoadData(); }, this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedEvent<ItemListRow>>(async eventArgs => { await this.LoadData(); });
            this.modelSelectionChangedSubscription =
                this.EventService.Subscribe<ModelSelectionChangedEvent<ItemListRow>>(
                    eventArgs =>
                    {
                        if (eventArgs.ModelId.HasValue)
                        {
                            this.Data = eventArgs.ModelId.Value;
                            this.LoadData();
                        }
                        else
                        {
                            this.ItemListRow = null;
                        }
                    },
                    this.Token,
                    true,
                    true);
        }

        private async Task LoadData()
        {
            if ((this.Data is int modelId))
            {
                this.ItemListRow = await this.itemListRowProvider.GetById(modelId);

                this.ItemsDataSource = this.itemListRow != null
                ? new DataSource<Item>(() => this.itemProvider.GetAll())
                : null;
            }
        }

        #endregion Methods
    }
}
