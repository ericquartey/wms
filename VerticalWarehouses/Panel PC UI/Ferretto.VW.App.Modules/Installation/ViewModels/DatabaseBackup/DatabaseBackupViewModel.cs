using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
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

        private readonly IMachineWmsStatusWebService machineWmsStatusWebService;

        private bool areSettingsChanged;

        private bool isBackupOnServer;

        private bool isBackupOnServerEnabled;

        private bool isBackupOnTelemetry;

        private bool isBackupOnTelemetryEnabled;

        private bool isBusy;

        private DelegateCommand saveCommand;

        #endregion

        #region Constructors

        public DatabaseBackupViewModel(IMachineDatabaseBackupWebService machineDatabaseBackupWebService,
            IMachineWmsStatusWebService machineWmsStatusWebService)
            : base(PresentationMode.Installer)
        {
            this.machineDatabaseBackupWebService = machineDatabaseBackupWebService ?? throw new ArgumentNullException(nameof(machineDatabaseBackupWebService));
            this.machineWmsStatusWebService = machineWmsStatusWebService ?? throw new ArgumentNullException(nameof(machineWmsStatusWebService));
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

            //set checkbox activated on page open
            this.IsBackupOnServerEnabled = true;
            this.IsBackupOnTelemetryEnabled = true;
        }

        protected override async Task OnDataRefreshAsync()
        {
            this.IsBackupOnServerEnabled = true; //await this.machineWmsStatusWebService.IsEnabledAsync();
            this.IsBackupOnTelemetryEnabled = true;

            this.IsBackupOnServer = await this.machineDatabaseBackupWebService.GetBackupOnServerAsync();
            this.IsBackupOnTelemetry = await this.machineDatabaseBackupWebService.GetBackupOnTelemetryAsync();

            this.IsBusy = false;
        }

        private bool CanSave()
        {
            return !this.IsBusy;
        }

        private async Task SaveAsync()
        {
            try
            {
                this.IsBusy = true;

                this.ClearNotifications();

                if (this.areSettingsChanged)
                {
                    await this.machineDatabaseBackupWebService.SetBackupOnServerAsync(this.IsBackupOnServer);
                    await this.machineDatabaseBackupWebService.SetBackupOnTelemetryAsync(this.IsBackupOnTelemetry);
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

        #endregion
    }
}
