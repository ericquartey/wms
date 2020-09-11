using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Maintenance)]
    public class UpdateStep1ViewModel : BaseUpdateViewModel
    {
        #region Fields

        private const string BAKUPZIPFILENAME = "F:\\Update\\Backup\\Backup.zip";

        private readonly IDialogService dialogService;

        private readonly DelegateCommand nextCommand;

        private readonly DelegateCommand restoreCommand;

        private DelegateCommand supportCommand;

        #endregion

        #region Constructors

        public UpdateStep1ViewModel(IDialogService dialogService, IUsbWatcherService usbWatcher)
            : base(usbWatcher)
        {
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            this.nextCommand = new DelegateCommand(() => this.GoNextStep(), this.CanGoNextStep);
            this.restoreCommand = new DelegateCommand(() => this.Restore(), this.CanRestore);
        }

        #endregion

        #region Properties

        public ICommand NextCommand => this.nextCommand;

        public ICommand RestoreCommand => this.restoreCommand;

        public ICommand SupportCommand =>
            this.supportCommand
            ??
            (this.supportCommand = new DelegateCommand(() => this.OpenSupport(),
                this.CanOpen));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.restoreCommand.RaiseCanExecuteChanged();
            this.nextCommand.RaiseCanExecuteChanged();

            base.RaiseCanExecuteChanged();
        }

        private bool CanGoNextStep() =>
            this.Installations.Any()
            &&
            !this.IsBusy;

        private bool CanOpen()
        {
            return true;
        }

        private bool CanRestore()
        {
            return File.Exists(BAKUPZIPFILENAME);
        }

        private void GoNextStep()
        {
            try
            {
                this.IsBusy = true;

                if (!this.Installations.Any())
                {
                    this.ShowNotification(Localized.Get("InstallationApp.FileReadError"), Services.Models.NotificationSeverity.Error);
                    this.IsBusy = false;
                    return;
                }

                this.NavigationService.Appear(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.Update.STEP2,
                    this.Installations,
                    trackCurrentView: false);
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

        private void OpenSupport()
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                var path = settings["SupportProgram"].Value;

                Process.Start(path);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void Restore()
        {
            var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmRestoreToPreviousVersion"), Localized.Get("InstallationApp.ConfirmRestore"), DialogType.Question, DialogButtons.YesNo);
            if (messageBoxResult is DialogResult.Yes)
            {
                try
                {
                    this.IsBackNavigationAllowed = false;

                    this.IsWaitingForResponse = true;

                    this.NavigationService.Appear(
                            nameof(Utils.Modules.Installation),
                            Utils.Modules.Installation.Update.STEP2,
                            BAKUPZIPFILENAME,
                            trackCurrentView: false);

                    this.RaiseCanExecuteChanged();
                }
                catch (Exception ex)
                {
                    this.ShowNotification(ex);
                }
                finally
                {
                    this.IsWaitingForResponse = false;
                    this.IsBackNavigationAllowed = true;
                }
            }
        }

        #endregion
    }
}
