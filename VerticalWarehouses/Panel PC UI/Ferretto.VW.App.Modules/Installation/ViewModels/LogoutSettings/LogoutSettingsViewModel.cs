using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class LogoutSettingsViewModel : BaseMainViewModel
    {
        #region Fields

        private bool isBusy;

        private DelegateCommand saveCommand;

        private LogoutSettings selectedLogoutSettings;

        private ObservableCollection<LogoutSettings> logoutSettings;

        private readonly IMachineLogoutSettingsWebService machineLogoutSettingsWebService;

        #endregion

        #region Constructors

        public LogoutSettingsViewModel(
            IMachineLogoutSettingsWebService machineLogoutSettingsWebService)
            : base(PresentationMode.Installer)
        {
            this.machineLogoutSettingsWebService = machineLogoutSettingsWebService ?? throw new ArgumentNullException(nameof(machineLogoutSettingsWebService));
        }

        #endregion

        #region Properties

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value, this.RaiseCanExecuteChanged);
        }

        public LogoutSettings SelectedLogoutSettings
        {
            get => this.selectedLogoutSettings;
            set => this.SetProperty(ref this.selectedLogoutSettings, value, this.RaiseCanExecuteChanged);
        }

        public ObservableCollection<LogoutSettings> LogoutSettings
        {
            get => this.logoutSettings;
            set => this.SetProperty(ref this.logoutSettings, value, this.RaiseCanExecuteChanged);
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
            this.IsBusy = true;

            await base.OnAppearedAsync();

            await this.Reload();

            this.IsBusy = false;

            this.RaiseCanExecuteChanged();
        }

        private async Task Reload()
        {
            this.LogoutSettings = IEnumConvert(await this.machineLogoutSettingsWebService.GetAllLogoutSettingsAsync());

            if (this.logoutSettings is null)
            {
                this.logoutSettings = new ObservableCollection<LogoutSettings>();
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsBusy));
            this.RaisePropertyChanged(nameof(this.LogoutSettings));
            this.RaisePropertyChanged(nameof(this.SelectedLogoutSettings));

            this.saveCommand?.RaiseCanExecuteChanged();
        }

        private bool CanSave()
        {
            return !this.IsBusy &&
                this.selectedLogoutSettings != null &&
                (this.selectedLogoutSettings.IsActive && this.selectedLogoutSettings.Timeout > 0);
        }

        public override void Disappear()
        {
            base.Disappear();

            this.LogoutSettings.Clear();
            this.SelectedLogoutSettings = null;
        }

        private async Task SaveAsync()
        {
            try
            {
                    this.IsBusy = true;

                    this.ClearNotifications();

                    await this.machineLogoutSettingsWebService.AddLogoutSettingsAsync(this.SelectedLogoutSettings);

                    this.ShowNotification(Localized.Get("InstallationApp.SaveSuccessful"));
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                await this.Reload();

                this.IsBusy = false;
            }
        }

        #endregion
    }
}
