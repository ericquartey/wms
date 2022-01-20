using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
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

        private bool areSettingsChanged;

        private DelegateCommand checkEndpointCommand;

        private int connectionTimeout;

        private HealthStatus healthStatus;

        private bool isAllDisabled;

        private bool isCheckingEndpoint;

        private bool isWmsEnabled;

        private DelegateCommand saveCommand;

        private bool socketLinkEndOfLine;

        private bool socketLinkIsEnabled;

        private int socketLinkPolling;

        private int socketLinkPort;

        private int socketLinkTimeout;

        private string wmsHttpUrl;

        private Brush wmsServicesStatusBrush;

        private string wmsServicesStatusDescription;

        #endregion

        #region Constructors

        public WmsSettingsViewModel(IMachineWmsStatusWebService wmsStatusWebService)
            : base(PresentationMode.Installer)
        {
            this.wmsStatusWebService = wmsStatusWebService ?? throw new ArgumentNullException(nameof(wmsStatusWebService));
        }

        #endregion

        #region Properties

        public bool AreSettingsChanged
        {
            get => this.areSettingsChanged;
            set
            {
                if (this.SetProperty(ref this.areSettingsChanged, value))
                {
                    if (this.AreSettingsChanged)
                    {
                        this.ClearNotifications();
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand CheckEndpointCommand =>
                    this.checkEndpointCommand
            ??
            (this.checkEndpointCommand = new DelegateCommand(
                async () => await this.CheckEndpointAsync(), this.CanCheckEndpoint));

        public int ConnectionTimeout
        {
            get => this.connectionTimeout;
            set
            {
                if (this.SetProperty(ref this.connectionTimeout, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public HealthStatus HealthStatus
        {
            get => this.healthStatus;
            private set
            {
                if (this.SetProperty(ref this.healthStatus, value))
                {
                    this.UpdateWmsStatus();
                }
            }
        }

        public bool IsAllDisabled
        {
            get => this.isAllDisabled;
            set
            {
                if (this.SetProperty(ref this.isAllDisabled, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsCheckingEndpoint
        {
            get => this.isCheckingEndpoint;
            private set
            {
                if (this.SetProperty(ref this.isCheckingEndpoint, value))
                {
                    this.UpdateWmsStatus();
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsWmsEnabled
        {
            get => this.isWmsEnabled;
            set
            {
                if (this.SetProperty(ref this.isWmsEnabled, value))
                {
                    if (!this.isWmsEnabled)
                    {
                        this.HealthStatus = HealthStatus.Unknown;
                    }

                    //if (this.isWmsEnabled)
                    //{
                    //    this.wmsStatusWebService.UpdateWmsTimeSettingsAsync();
                    //}

                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand SaveCommand =>
            this.saveCommand
            ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.SaveAsync(), this.CanSave));

        public bool SocketLinkEndOfLine
        {
            get => this.socketLinkEndOfLine;
            set
            {
                if (this.SetProperty(ref this.socketLinkEndOfLine, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool SocketLinkIsEnabled
        {
            get => this.socketLinkIsEnabled;
            set
            {
                if (this.SetProperty(ref this.socketLinkIsEnabled, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int SocketLinkPolling
        {
            get => this.socketLinkPolling;
            set
            {
                if (this.SetProperty(ref this.socketLinkPolling, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int SocketLinkPort
        {
            get => this.socketLinkPort;
            set
            {
                if (this.SetProperty(ref this.socketLinkPort, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public int SocketLinkTimeout
        {
            get => this.socketLinkTimeout;
            set
            {
                if (this.SetProperty(ref this.socketLinkTimeout, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string WmsHttpUrl
        {
            get => this.wmsHttpUrl;
            set
            {
                if (this.SetProperty(ref this.wmsHttpUrl, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public Brush WmsServicesStatusBrush
        {
            get => this.wmsServicesStatusBrush;
            set => this.SetProperty(ref this.wmsServicesStatusBrush, value);
        }

        public string WmsServicesStatusDescription
        {
            get => this.wmsServicesStatusDescription;
            set => this.SetProperty(ref this.wmsServicesStatusDescription, value);
        }

        #endregion

        #region Methods

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                //this.IsAllDisabled = false;

                this.IsWmsEnabled = await this.wmsStatusWebService.IsEnabledAsync();
                this.WmsHttpUrl = await this.wmsStatusWebService.GetIpEndpointAsync();
                this.ConnectionTimeout = await this.wmsStatusWebService.GetConnectionTimeoutAsync();

                this.SocketLinkIsEnabled = await this.wmsStatusWebService.SocketLinkIsEnabledAsync();
                this.SocketLinkPort = await this.wmsStatusWebService.GetSocketLinkPortAsync();
                this.SocketLinkTimeout = await this.wmsStatusWebService.GetSocketLinkTimeoutAsync();
                this.SocketLinkPolling = await this.wmsStatusWebService.GetSocketLinkPollingAsync();
                this.SocketLinkEndOfLine = await this.wmsStatusWebService.GetSocketLinkEndOfLineAsync();

                //if (!this.IsWmsEnabled && !this.SocketLinkIsEnabled)
                //{
                //    this.IsAllDisabled = true;
                //}

                if (this.SocketLinkIsEnabled)
                {
                    this.RaisePropertyChanged(nameof(this.SocketLinkIsEnabled));
                }
                else if (this.IsWmsEnabled)
                {
                    this.RaisePropertyChanged(nameof(this.IsWmsEnabled));
                }
                else
                {
                    this.RaisePropertyChanged(nameof(this.IsAllDisabled));
                }

                this.AreSettingsChanged = false;

                await this.CheckEndpointAsync();
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
            return
                !this.IsCheckingEndpoint
                &&
                !this.AreSettingsChanged
                &&
                this.IsWmsEnabled
                &&
                this.WmsHttpUrl != null;
        }

        private bool CanSave()
        {
            return
                //!this.IsWaitingForResponse
                //&&
                this.AreSettingsChanged;
        }

        private async Task CheckEndpointAsync()
        {
            if (!this.IsWmsEnabled || this.WmsHttpUrl is null)
            {
                this.HealthStatus = HealthStatus.Unknown;
                return;
            }

            try
            {
                this.ClearNotifications();
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

                await this.wmsStatusWebService.UpdateAsync(this.IsWmsEnabled, this.WmsHttpUrl, this.SocketLinkIsEnabled, this.SocketLinkPort, this.SocketLinkTimeout, this.SocketLinkPolling, this.ConnectionTimeout, this.SocketLinkEndOfLine);
                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.InformationSuccessfullyUpdated"));
                this.AreSettingsChanged = false;

                this.IsWaitingForResponse = false;

                if (this.IsWmsEnabled)
                {
                    await this.CheckEndpointAsync();
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void UpdateWmsStatus()
        {
            if (this.isCheckingEndpoint)
            {
                this.WmsServicesStatusDescription = VW.App.Resources.Localized.Get("InstallationApp.WmsStatusCheckInProgress");
                this.WmsServicesStatusBrush = Brushes.Gray;
            }
            else if (this.healthStatus is HealthStatus.Healthy)
            {
                this.WmsServicesStatusDescription = VW.App.Resources.Localized.Get("InstallationApp.WmsStatusOnline");
                this.WmsServicesStatusBrush = Brushes.Green;
            }
            else if (this.healthStatus is HealthStatus.Degraded)
            {
                this.WmsServicesStatusDescription = VW.App.Resources.Localized.Get("InstallationApp.WmsStatusOnline");
                this.WmsServicesStatusBrush = Brushes.Gold;
            }
            else if (this.healthStatus is HealthStatus.Unhealthy)
            {
                this.WmsServicesStatusDescription = VW.App.Resources.Localized.Get("InstallationApp.WmsStatusOffline");
                this.WmsServicesStatusBrush = Brushes.Red;

                //this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.WmsOffline"), Services.Models.NotificationSeverity.Warning);
            }
            else
            {
                this.WmsServicesStatusDescription = VW.App.Resources.Localized.Get("InstallationApp.Unknown");
                this.WmsServicesStatusBrush = Brushes.Gray;
            }
        }

        #endregion
    }
}
