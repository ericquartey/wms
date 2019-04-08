using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.WMS.App.Core.Interfaces;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitWithdrawDialogViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

        private ICommand closeDialogCommand;

        private string error;

        private int loadingUnitId;

        private ICommand runWithdrawCommand;

        #endregion

        #region Properties

        public ICommand CloseDialogCommand => this.closeDialogCommand ??
                     (this.closeDialogCommand = new DelegateCommand(
                 this.ExecuteCloseDialogCommand));

        public string Error { get => this.error; set => this.SetProperty(ref this.error, value); }

        public ICommand RunWithdrawCommand => this.runWithdrawCommand ??
                    (this.runWithdrawCommand = new DelegateCommand(
                    async () => await this.RunWithdrawAsync()));

        #endregion

        #region Methods

        protected void ExecuteCloseDialogCommand()
        {
            this.Disappear();
        }

        protected override async Task OnAppearAsync()
        {
            await base.OnAppearAsync().ConfigureAwait(true);

            this.LoadData();
        }

        private void LoadData()
        {
            if (this.Data is int)
            {
                this.loadingUnitId = (int)this.Data;
            }
        }

        private async Task RunWithdrawAsync()
        {
            if (this.loadingUnitId == 0)
            {
                return;
            }

            var result = await this.loadingUnitProvider.WithdrawAsync(this.loadingUnitId);

            if (result.Success)
            {
                this.EventService.Invoke(new StatusPubSubEvent(Common.Resources.MasterData.LoadingUnitWithdrawCommenced, StatusType.Success));

                this.Disappear();
            }
            else
            {
                this.Error = result.Description;
                this.EventService.Invoke(new StatusPubSubEvent(result.Description, StatusType.Error));
            }
        }

        #endregion
    }
}
