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

        private IPEndPoint ipEndpoint;

        private bool isCheckingEndpoint;

        private bool isWmsEnabled;

        private DelegateCommand saveCommand;

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

        public HealthStatus HealthStatus
        {
            get => this.healthStatus;
            private set => this.SetProperty(ref this.healthStatus, value);
        }

        public IPEndPoint IPEndPoint
        {
            get => this.ipEndpoint;
            set => this.SetProperty(ref this.ipEndpoint, value);
        }

        public bool IsCheckingEndpoint
        {
            get => this.isCheckingEndpoint;
            private set => this.SetProperty(ref this.isCheckingEndpoint, value);
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

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
        }

        private bool CanCheckEndpoint()
        {
            return !this.IsCheckingEndpoint;
        }

        private bool CanSave()
        {
            return true;
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
            catch (MasWebApiException<ProblemDetails> ex)
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
                await this.wmsStatusWebService.UpdateAsync(this.IsEnabled, this.IPEndPoint.Address.ToString(), this.IPEndPoint.Port);
                this.ShowNotification(VW.App.Resources.InstallationApp.InformationSuccessfullyUpdated);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
