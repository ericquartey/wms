using System;
using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.ItemLists
{
    public class ItemListAddDialogViewModel : CreateViewModel<ItemListDetails>
    {
        #region Fields

        private readonly IItemListProvider itemListProvider = ServiceLocator.Current.GetInstance<IItemListProvider>();

        #endregion

        #region Methods

        protected override async Task ExecuteClearCommandAsync()
        {
            await Task.Run(() => this.LoadData());
        }

        protected override Task<bool> ExecuteCompleteCommandAsync()
        {
            throw new NotSupportedException();
        }

        protected override async Task<bool> ExecuteCreateCommandAsync()
        {
            if (!this.CheckValidModel())
            {
                return false;
            }

            this.IsBusy = true;

            var resultCreate = await this.itemListProvider.CreateAsync(this.Model);
            if (resultCreate.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.ItemLists.ItemListSavedSuccessfully, StatusType.Success));

                this.CloseDialogCommand.Execute(null);
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;

            return resultCreate.Success;
        }

        protected override async Task OnAppearAsync()
        {
            await base.OnAppearAsync().ConfigureAwait(true);

            this.LoadData();
        }

        private void LoadData()
        {
            try
            {
                this.IsBusy = true;
                this.Model = this.itemListProvider.GetNew();
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
