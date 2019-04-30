using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.WMS.App.Controls;
using Ferretto.WMS.App.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitWithdrawDialogViewModel : BaseDialogViewModel<LoadingUnitWithdraw>
    {
        #region Fields

        private readonly IBayProvider bayProvider = ServiceLocator.Current.GetInstance<IBayProvider>();

        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

        private ICommand runWithdrawCommand;

        #endregion

        #region Properties

        public ICommand RunWithdrawCommand => this.runWithdrawCommand ??
            (this.runWithdrawCommand = new DelegateCommand(
                    async () => await this.RunWithdrawAsync(),
                    this.CanRunWithdraw)
                .ObservesProperty(() => this.Model));

        #endregion

        #region Methods

        protected override void EvaluateCanExecuteCommands()
        {
            ((DelegateCommand)this.RunWithdrawCommand)?.RaiseCanExecuteChanged();
        }

        protected override async Task OnAppearAsync()
        {
            this.IsBusy = true;

            await base.OnAppearAsync().ConfigureAwait(true);
            await this.LoadDataAsync();

            this.IsBusy = false;
        }

        private bool CanRunWithdraw()
        {
            return !this.IsBusy;
        }

        private async Task LoadDataAsync()
        {
            var modelId = (int?)this.Data.GetType().GetProperty("LoadingUnitId")?.GetValue(this.Data);
            if (!modelId.HasValue)
            {
                return;
            }

            var luDetails = await this.loadingUnitProvider.GetByIdAsync((int)modelId);
            var bayChoices = luDetails.AreaId.HasValue
                ? await this.bayProvider.GetByAreaIdAsync(luDetails.AreaId.Value)
                : null;
            this.Model = new LoadingUnitWithdraw
            {
                IsValidationEnabled = false,
                Id = luDetails.Id,
                Code = luDetails.Code,
                LoadingUnitTypeDescription = luDetails.LoadingUnitTypeDescription,
                LoadingUnitStatusDescription = luDetails.LoadingUnitStatusDescription,
                AreaId = luDetails.AreaId,
                AreaName = luDetails.AreaName,
                BayChoices = bayChoices,
            };
        }

        private async Task RunWithdrawAsync()
        {
            if (!this.CheckValidModel())
            {
                return;
            }

            this.IsBusy = true;

            var result = await this.loadingUnitProvider.WithdrawAsync(this.Model.Id, this.Model.BayId.Value);

            this.IsBusy = false;

            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(
                    Common.Resources.MasterData.LoadingUnitWithdrawCommenced,
                    StatusType.Success));

                this.CloseDialogCommand.Execute(null);
            }
            else
            {
                this.EventService.Invoke(new StatusPubSubEvent(
                    result.Description,
                    StatusType.Error));
            }
        }

        #endregion
    }
}
