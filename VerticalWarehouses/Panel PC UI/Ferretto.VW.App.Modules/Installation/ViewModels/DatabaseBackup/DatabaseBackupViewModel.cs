using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Telemetry.Contracts.Hub;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class DatabaseBackupViewModel : BaseMainViewModel
    {

        #region Fields

        private readonly IMachineDatabaseBackupWebService machineDatabaseBackupWebService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly ITelemetryHubClient telemetryHubClient;

        private bool areSettingsChanged;

        private bool isBackupOnServer;

        private bool isBackupOnServerEnabled;

        private bool isBackupOnTelemetry;

        private bool isBackupOnTelemetryEnabled;

        private bool isBusy;

        private bool isStandbyDbOk;

        private string proxyPassword;

        private string proxyUrl;

        private string proxyUser;

        private DelegateCommand saveCommand;

        private string serverPassword;

        private string serverPath;

        private string serverUsername;

        private DelegateCommand testCommand;

        #endregion Fields

        #region Constructors

        public DatabaseBackupViewModel(IMachineDatabaseBackupWebService machineDatabaseBackupWebService,
            IMachineIdentityWebService machineIdentityWebService,
            ITelemetryHubClient telemetryHubClient)
            : base(PresentationMode.Installer)
        {
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
            this.machineDatabaseBackupWebService = machineDatabaseBackupWebService ?? throw new ArgumentNullException(nameof(machineDatabaseBackupWebService));
            this.telemetryHubClient = telemetryHubClient ?? throw new ArgumentNullException(nameof(telemetryHubClient));
            this.telemetryHubClient.ProxyReceivedChanged += async (sender, e) => await this.OnProxyReceivedChangedAsync(sender, e);
        }

        #endregion Constructors

        #region Properties

        public bool IsBackupOnServer
        {
            get => this.isBackupOnServer;
            set
            {
                if (this.SetProperty(ref this.isBackupOnServer, value))
                {
                    this.areSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsBackupOnServerEnabled
        {
            get => this.isBackupOnServerEnabled;
            set
            {
                if (this.SetProperty(ref this.isBackupOnServerEnabled, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsBackupOnTelemetry
        {
            get => this.isBackupOnTelemetry;
            set
            {
                if (this.SetProperty(ref this.isBackupOnTelemetry, value))
                {
                    this.areSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsBackupOnTelemetryEnabled
        {
            get => this.isBackupOnTelemetryEnabled;
            set
            {
                if (this.SetProperty(ref this.isBackupOnTelemetryEnabled, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    ((DelegateCommand)this.saveCommand)?.RaiseCanExecuteChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        public bool IsEnabled => true;

        public bool IsStandbyDbOk
        {
            get => this.isStandbyDbOk;
            set
            {
                if (this.SetProperty(ref this.isStandbyDbOk, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string ProxyPassword
        {
            get => this.proxyPassword;
            set => this.SetProperty(ref this.proxyPassword, value);
        }

        public string ProxyUrl
        {
            get => this.proxyUrl;
            set => this.SetProperty(ref this.proxyUrl, value);
        }

        public string ProxyUser
        {
            get => this.proxyUser;
            set => this.SetProperty(ref this.proxyUser, value);
        }

        public ICommand SaveCommand =>
                    this.saveCommand
            ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.SaveAsync(), this.CanSave));

        public string ServerPassword
        {
            get => this.serverPassword;
            set
            {
                if (this.SetProperty(ref this.serverPassword, value))
                {
                    //this.areSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string ServerPath
        {
            get => this.serverPath;
            set
            {
                if (this.SetProperty(ref this.serverPath, value))
                {
                    //this.areSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string ServerUsername
        {
            get => this.serverUsername;
            set
            {
                if (this.SetProperty(ref this.serverUsername, value))
                {
                    //this.areSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand TestCommand =>
                    this.testCommand
            ??
            (this.testCommand = new DelegateCommand(
                async () => await this.TestAsync(), this.CanTest));

        #endregion Properties

        #region Methods

        public override async void Disappear()
        {
            base.Disappear();

            if (this.IsBackupOnTelemetry)
            {
                await this.telemetryHubClient.GetProxyAsync();
            }
            else
            {
                this.ProxyPassword = string.Empty;
                this.ProxyUrl = string.Empty;
                this.ProxyUser = string.Empty;
            }
        }

        public override async Task OnAppearedAsync()
        {
            await this.telemetryHubClient.GetProxyAsync();
            await base.OnAppearedAsync();
            Task.Run(async () =>
            {
                do
                {
                    await Task.Delay(800);
                    this.IsStandbyDbOk = await this.machineDatabaseBackupWebService.GetStandbyDbAsync();
                }
                while (this.IsVisible);
            });

            if (this.IsBackupOnTelemetry)
            {
                await this.telemetryHubClient.GetProxyAsync();
            }
            else
            {
                this.ProxyPassword = string.Empty;
                this.ProxyUrl = string.Empty;
                this.ProxyUser = string.Empty;
            }
        }

        protected override async Task OnDataRefreshAsync()
        {
            this.IsBackupOnServerEnabled = true; //await this.machineWmsStatusWebService.IsEnabledAsync();
            this.IsBackupOnTelemetryEnabled = true;

            this.IsBackupOnServer = await this.machineDatabaseBackupWebService.GetBackupOnServerAsync();
            this.IsBackupOnTelemetry = await this.machineDatabaseBackupWebService.GetBackupOnTelemetryAsync();
            this.IsStandbyDbOk = await this.machineDatabaseBackupWebService.GetStandbyDbAsync();

            this.ServerPath = await this.machineDatabaseBackupWebService.GetBackupServerAsync();
            this.ServerPassword = await this.machineDatabaseBackupWebService.GetBackupServerPasswordAsync();
            this.ServerUsername = await this.machineDatabaseBackupWebService.GetBackupServerUsernameAsync();

            this.IsKeyboardButtonVisible = await this.machineIdentityWebService.GetTouchHelperEnableAsync();

            this.IsBusy = false;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.saveCommand?.RaiseCanExecuteChanged();
            this.testCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsKeyboardButtonVisible));
        }

        private bool CanSave()
        {
            return !this.IsBusy;
        }

        private bool CanTest()
        {
            return !this.IsBusy &&
                !string.IsNullOrEmpty(this.serverPath) &&
                !string.IsNullOrEmpty(this.serverUsername);
        }

        private async Task OnProxyReceivedChangedAsync(object sender, Common.Hubs.ProxyChangedEventArgs e)
        {
            var proxy = e.Proxy;
            if (proxy is null)
            {
                this.ProxyPassword = string.Empty;
                this.ProxyUrl = string.Empty;
                this.ProxyUser = string.Empty;
            }
            else
            {
                this.ProxyPassword = DecryptEncrypt.Decrypt(proxy.PasswordHash, proxy.PasswordSalt);
                this.ProxyUrl = proxy.Url;
                this.ProxyUser = proxy.User;
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                this.IsBusy = true;

                this.ClearNotifications();

                if (this.areSettingsChanged)
                {
                    await this.machineDatabaseBackupWebService.SetBackupOnServerAsync(this.IsBackupOnServer, this.serverPath, this.serverUsername, this.serverPassword);
                    await this.machineDatabaseBackupWebService.SetBackupOnTelemetryAsync(this.IsBackupOnTelemetry);

                    if (this.IsBackupOnTelemetry && !string.IsNullOrEmpty(this.ProxyUrl))
                    {
                        if (!string.IsNullOrEmpty(this.ProxyUser) && !string.IsNullOrEmpty(this.ProxyPassword))
                        {
                            var salt = Convert.ToBase64String(DecryptEncrypt.GeneratePasswordSalt());

                            var hash = DecryptEncrypt.Encrypt(this.ProxyPassword, salt);

                            var proxy = new Proxy() { Url = this.ProxyUrl, User = this.ProxyUser, PasswordHash = hash, PasswordSalt = salt };

                            await this.telemetryHubClient.SendProxyAsync(proxy);

                            this.ShowNotification(Localized.Get("InstallationApp.SaveSuccessful") + ", " + Localized.Get("InstallationApp.ProxySaved"), Services.Models.NotificationSeverity.Success);
                        }
                        else
                        {
                            this.ShowNotification(Localized.Get("InstallationApp.SaveSuccessful"), Services.Models.NotificationSeverity.Info);
                        }
                    }
                    else
                    {
                        await this.telemetryHubClient.SendProxyAsync(null);
                        this.ProxyPassword = "";
                        this.ProxyUrl = "";
                        this.ProxyUser = "";
                        this.ShowNotification(Localized.Get("InstallationApp.SaveSuccessful"), Services.Models.NotificationSeverity.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                await this.telemetryHubClient.SendProxyAsync(null);
                this.ProxyPassword = "";
                this.ProxyUrl = "";
                this.ProxyUser = "";
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private async Task TestAsync()
        {
            try
            {
                this.IsBusy = true;

                this.ClearNotifications();

                await this.machineDatabaseBackupWebService.TestBackupOnServerAsync(this.serverPath, this.serverUsername, this.serverPassword);

                this.ShowNotification(Localized.Get("InstallationApp.SaveSuccessful"));
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }
        #endregion Methods

    }
}
