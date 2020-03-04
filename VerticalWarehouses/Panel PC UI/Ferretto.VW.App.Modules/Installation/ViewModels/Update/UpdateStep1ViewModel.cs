using System;
using System.IO;
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

        private const string BAKUPZIPFILENAME = "F:\\Update\\Backup\\Current_Version_Backup.zip";

        private readonly IDialogService dialogService;

        private DelegateCommand nextCommand;

        private DelegateCommand restoreCommand;

        #endregion

        #region Constructors

        public UpdateStep1ViewModel(IDialogService dialogService)
            : base()
        {
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        #endregion

        #region Properties

        public ICommand NextCommand =>
                        this.nextCommand
                        ??
                        (this.nextCommand = new DelegateCommand(() => this.GoNextStep(), this.CanGoNextStep));

        public ICommand RestoreCommand =>
                        this.restoreCommand
                        ??
                        (this.restoreCommand = new DelegateCommand(() => this.Restore(), this.CanRestore));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;
        }

        public override void RaisePropertyChanged()
        {
            base.RaisePropertyChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.restoreCommand.RaiseCanExecuteChanged();
            this.nextCommand.RaiseCanExecuteChanged();
            base.RaiseCanExecuteChanged();
        }

        private bool CanGoNextStep()
        {
            return this.Installations.Count > 0
                   &&
                   this.IsInstallationReady;
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

                if (this.Installations.Count == 0)
                {
                    this.ShowNotification(InstallationApp.FileReadError, Services.Models.NotificationSeverity.Error);
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

        private void Restore()
        {
            var messageBoxResult = this.dialogService.ShowMessage(InstallationApp.ConfirmRestoreToPreviousVersion, InstallationApp.ConfirmRestore, DialogType.Question, DialogButtons.YesNo);
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
