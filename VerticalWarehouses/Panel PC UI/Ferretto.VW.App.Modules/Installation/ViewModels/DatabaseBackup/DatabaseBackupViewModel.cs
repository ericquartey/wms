using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Ferretto.VW.Telemetry.Contracts.Hub;
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

        private DelegateCommand saveCommand;

        private string serverPassword;

        private string serverPath;

        private string serverUsername;

        private DelegateCommand testCommand;

        #endregion

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

        #endregion

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

        #endregion

        #region Methods

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
                    await this.telemetryHubClient.SendProxyAsync(null);     // todo
                }

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

        #endregion
    }
}
