using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommonServiceLocator;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Core.Interfaces;
using Ferretto.WMS.App.Core.Models;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitWithdrawDialogViewModel : BaseServiceNavigationViewModel
    {
        #region Fields

        private readonly IBayProvider bayProvider = ServiceLocator.Current.GetInstance<IBayProvider>();

        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();

        private bool canShowError;

        private ICommand closeDialogCommand;

        private string error;

        private bool isBusy;

        private bool isEnableError;

        private bool isModelValid;

        private LoadingUnitWithdraw loadingUnitWithdraw;

        private ICommand runWithdrawCommand;

        #endregion

        #region Properties

        public bool CanShowError
        {
            get => this.canShowError;
            set
            {
                this.SetProperty(ref this.canShowError, value);
                this.UpdateIsEnableError();
            }
        }

        public ICommand CloseDialogCommand => this.closeDialogCommand ??
                     (this.closeDialogCommand = new DelegateCommand(
                 this.ExecuteCloseDialogCommand));

        public string Error { get => this.error; set => this.SetProperty(ref this.error, value); }

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public bool IsEnableError
        {
            get => this.isEnableError;
            set => this.SetProperty(ref this.isEnableError, value);
        }

        public bool IsModelValid
        {
            get
            {
                var temp = false;
                if (this.LoadingUnitWithdraw == null)
                {
                    temp = true;
                }
                else
                {
                    temp = string.IsNullOrWhiteSpace(this.LoadingUnitWithdraw.Error);
                }

                this.SetProperty(ref this.isModelValid, temp);
                this.UpdateIsEnableError();
                return temp;
            }
        }

        public LoadingUnitWithdraw LoadingUnitWithdraw
        {
            get => this.loadingUnitWithdraw;
            set
            {
                if (this.LoadingUnitWithdraw != null && value != this.LoadingUnitWithdraw)
                {
                    this.LoadingUnitWithdraw.PropertyChanged -= this.OnLoadingUnitWithdrawPropertyChanged;
                }

                if (this.SetProperty(ref this.loadingUnitWithdraw, value))
                {
                    this.LoadingUnitWithdraw.PropertyChanged += this.OnLoadingUnitWithdrawPropertyChanged;
                }
            }
        }

        public ICommand RunWithdrawCommand => this.runWithdrawCommand ??
                    (this.runWithdrawCommand = new DelegateCommand(
                    async () => await this.RunWithdrawAsync(),
                    this.CanRunWithdraw));

        #endregion

        #region Methods

        protected virtual void EvaluateCanExecuteCommands()
        {
            ((DelegateCommand)this.RunWithdrawCommand)?.RaiseCanExecuteChanged();
        }

        protected void ExecuteCloseDialogCommand()
        {
            this.Disappear();
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
            var canExecute = this.LoadingUnitWithdraw != null
               && this.IsModelValid
               && !this.IsBusy;

            if (canExecute)
            {
                this.CanShowError = true;
            }

            return canExecute;
        }

        private async Task LoadDataAsync()
        {
            var modelId = (int)this.Data.GetType().GetProperty("LoadingUnitId")?.GetValue(this.Data);

            var luDetails = await this.loadingUnitProvider.GetByIdAsync(modelId);
            var bayChoices = luDetails.AreaId.HasValue ?
                                                  await this.bayProvider.GetByAreaIdAsync(luDetails.AreaId.Value) :
                                                  null;
            this.LoadingUnitWithdraw = new LoadingUnitWithdraw
            {
                Id = luDetails.Id,
                Code = luDetails.Code,
                LoadingUnitTypeDescription = luDetails.LoadingUnitTypeDescription,
                LoadingUnitStatusDescription = luDetails.LoadingUnitStatusDescription,
                AreaId = luDetails.AreaId,
                AreaName = luDetails.AreaName,
                BayChoices = bayChoices,
            };
        }

        private void OnLoadingUnitWithdrawPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ((DelegateCommand)this.RunWithdrawCommand)?.RaiseCanExecuteChanged();
        }

        private async Task RunWithdrawAsync()
        {
            if (this.LoadingUnitWithdraw?.Id == 0 || !this.LoadingUnitWithdraw.BayId.HasValue)
            {
                return;
            }

            this.IsBusy = true;

            var result = await this.loadingUnitProvider.WithdrawAsync(this.LoadingUnitWithdraw.Id, this.LoadingUnitWithdraw.BayId.Value);

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

            this.IsBusy = false;
        }

        private void UpdateIsEnableError()
        {
            this.IsEnableError = this.isModelValid && this.CanShowError;
        }

        #endregion
    }
}
