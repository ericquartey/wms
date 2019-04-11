using System.Threading.Tasks;
using CommonServiceLocator;
using DevExpress.Xpf.Data;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemListRowAddDialogViewModel : CreateViewModel<ItemListRowDetails>
    {
        #region Fields

        private readonly IItemListRowProvider itemListRowProvider = ServiceLocator.Current.GetInstance<IItemListRowProvider>();

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        private InfiniteAsyncSource itemsDataSource;

        #endregion

        #region Properties

        public InfiniteAsyncSource ItemsDataSource
        {
            get => this.itemsDataSource;
            set => this.SetProperty(ref this.itemsDataSource, value);
        }

        #endregion

        #region Methods

        protected override async Task ExecuteClearCommandAsync()
        {
            await this.LoadDataAsync();
        }

        protected override async Task ExecuteCreateCommandAsync()
        {
            this.IsBusy = true;

            var result = await this.itemListRowProvider.CreateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedPubSubEvent<ItemList, int>(this.Model.Id));
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemSavedSuccessfully, StatusType.Success));

                this.CloseDialogCommand.Execute(null);
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

            await this.LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                this.IsBusy = true;
                this.ItemsDataSource = null;

                if (this.Data is int listId)
                {
                    this.Model = await this.itemListRowProvider.GetNewAsync(listId);
                    this.ItemsDataSource = new InfiniteDataSourceService<Item, int>(this.itemProvider).DataSource;
                }
            }
            catch
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToLoadData, StatusType.Error));
            }

            this.IsBusy = false;
        }

        #endregion
    }
}
