using System.Threading.Tasks;
using CommonServiceLocator;
using DevExpress.Xpf.Data;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Ferretto.WMS.App.Resources;

namespace Ferretto.WMS.App.Modules.ItemLists
{
    public class ItemListRowAddViewModel : CreateViewModel<ItemListRowDetails>
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

        protected override async Task<bool> ExecuteCreateCommandAsync()
        {
            if (!this.CheckValidModel())
            {
                return false;
            }

            this.IsBusy = true;

            var resultCreate = await this.itemListRowProvider.CreateAsync(this.Model);
            if (resultCreate.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new StatusPubSubEvent(App.Resources.ItemLists.ItemListRowSavedSuccessfully, StatusType.Success));

                this.CloseDialogCommand.Execute(null);
            }
            else
            {
                this.EventService.Invoke(
                    new StatusPubSubEvent(
                        Errors.UnableToSaveChanges,
                        resultCreate.Description,
                        StatusType.Error));
            }

            this.IsBusy = false;

            return resultCreate.Success;
        }

        protected override async Task OnAppearAsync()
        {
            await base.OnAppearAsync().ConfigureAwait(true);

            await this.LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            this.IsBusy = true;
            this.ItemsDataSource = null;

            if (this.Data is int listId)
            {
                var result = await this.itemListRowProvider.GetNewAsync(listId);

                if (result.Success)
                {
                    this.Model = result.Entity;
                    this.ItemsDataSource = new InfiniteDataSourceService<Item, int>(this.itemProvider).DataSource;
                }
                else
                {
                    this.EventService.Invoke(new StatusPubSubEvent(Errors.UnableToLoadData, StatusType.Error));
                }
            }

            this.IsBusy = false;
        }

        #endregion
    }
}
