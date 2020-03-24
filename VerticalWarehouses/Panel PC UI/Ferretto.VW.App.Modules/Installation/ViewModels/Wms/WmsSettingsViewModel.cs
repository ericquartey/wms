using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal class WmsSettingsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineWmsStatusWebService wmsStatusWebService;

        private DelegateCommand checkEndpointCommand;

        private HealthStatus healthStatus;

        private bool isCheckingEndpoint;

        private bool isWmsEnabled;

        private DelegateCommand saveCommand;

        private string wmsHttpUrl;

        #endregion

        #region Constructors

        public WmsSettingsViewModel(IMachineWmsStatusWebService wmsStatusWebService)
            : base(PresentationMode.Installer)
        {
            this.wmsStatusWebService = wmsStatusWebService ?? throw new ArgumentNullException(nameof(wmsStatusWebService));
        }

        #endregion

        #region Properties

        public ICommand CheckEndpointCommand =>
            this.checkEndpointCommand
            ??
            (this.checkEndpointCommand = new DelegateCommand(
                async () => await this.CheckEndpointAsync(), this.CanCheckEndpoint));

        public override EnableMask EnableMask => EnableMask.Any;

        public HealthStatus HealthStatus
        {
            get => this.healthStatus;
            private set => this.SetProperty(ref this.healthStatus, value);
        }

        public bool IsCheckingEndpoint
        {
            get => this.isCheckingEndpoint;
            private set
            {
                if (this.SetProperty(ref this.isCheckingEndpoint, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsWmsEnabled
        {
            get => this.isWmsEnabled;
            set => this.SetProperty(ref this.isWmsEnabled, value);
        }

        public ICommand SaveCommand =>
            this.saveCommand
            ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.SaveAsync(), this.CanSave));

        public string WmsHttpUrl
        {
            get => this.wmsHttpUrl;
            set
            {
                if (this.SetProperty(ref this.wmsHttpUrl, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Methods

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                this.IsWmsEnabled = await this.wmsStatusWebService.IsEnabledAsync();
                this.WmsHttpUrl = await this.wmsStatusWebService.GetIpEndpointAsync();

                if (this.IsWmsEnabled && this.WmsHttpUrl != null)
                {
                    await this.CheckEndpointAsync();
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }

            await base.OnDataRefreshAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.saveCommand?.RaiseCanExecuteChanged();
            this.checkEndpointCommand?.RaiseCanExecuteChanged();

            base.RaiseCanExecuteChanged();
        }

        private bool CanCheckEndpoint()
        {
            return !this.IsCheckingEndpoint;
        }

        private bool CanSave()
        {
            return
                this.WmsHttpUrl != null
                &&
                !this.IsWaitingForResponse;
        }

        private async Task CheckEndpointAsync()
        {
            try
            {
                this.IsCheckingEndpoint = true;

                var statusString = await this.wmsStatusWebService.GetHealthAsync();
                if (Enum.TryParse<HealthStatus>(statusString, out var status))
                {
                    this.HealthStatus = status;
                }
                else
                {
                    this.HealthStatus = HealthStatus.Unknown;
                }
            }
            catch (MasWebApiException)
            {
                this.HealthStatus = HealthStatus.Unhealthy;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsCheckingEndpoint = false;
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.wmsStatusWebService.UpdateAsync(this.IsEnabled, this.WmsHttpUrl);
                this.ShowNotification(VW.App.Resources.InstallationApp.InformationSuccessfullyUpdated);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;

                await this.CheckEndpointAsync();
            }
        }

        #endregion
    }
}
