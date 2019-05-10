using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;

namespace Ferretto.WMS.Modules.MasterData
{
    public class ItemAddViewModel : CreateViewModel<ItemDetails>
    {
        #region Fields

        private readonly IItemProvider itemProvider = ServiceLocator.Current.GetInstance<IItemProvider>();

        #endregion

        #region Methods

        protected override async Task ExecuteClearCommandAsync()
        {
            await this.LoadDataAsync();
        }

        protected override async Task<bool> ExecuteCompleteCommandAsync()
        {
            var resultUpdate = await this.itemProvider.UpdateAsync(this.Model);
            if (resultUpdate.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.ItemSavedSuccessfully, StatusType.Success));

                this.CloseDialogCommand.Execute(null);
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.Errors.UnableToSaveChanges, StatusType.Error));
            }

            this.IsBusy = false;

            return resultUpdate.Success;
        }

        protected override async Task<bool> ExecuteCreateCommandAsync()
        {
            if (!this.CheckValidModel())
            {
                return false;
            }

            this.IsBusy = true;

            var resultCreate = await this.itemProvider.CreateAsync(this.Model);
            if (resultCreate.Success)
            {
                this.TakeModelSnapshot();
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

            await this.LoadDataAsync().ConfigureAwait(true);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                this.IsBusy = true;
                this.Model = await this.itemProvider.GetNewAsync();
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
