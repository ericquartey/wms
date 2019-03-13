using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitAddDialogViewModel : CreateViewModel<LoadingUnitDetails>
    {
        #region Fields

        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

        #endregion

        #region Methods

        protected override async void ExecuteClearCommand()
        {
            await this.LoadDataAsync();
        }

        protected override async Task ExecuteCreateCommand()
        {
            this.IsBusy = true;

            var result = await this.loadingUnitProvider.CreateAsync(this.Model);
            if (result.Success)
            {
                this.TakeModelSnapshot();

                this.EventService.Invoke(new ModelChangedPubSubEvent<LoadingUnit, int>(this.Model.Id));
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.LoadingUnitSavedSuccessfully, StatusType.Success));

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
            await this.LoadDataAsync().ConfigureAwait(true);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                this.IsBusy = true;
                this.Model = await this.loadingUnitProvider.GetNewAsync();

                this.IsBusy = false;
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
