using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class UpdateStep2ViewModel : BaseMainViewModel
    {
        #region Fields

        private const string CSVEXTENSION = ".csv";

        private const string RESTOREARG = "--restore";

        private const string UPDATEARG = "--update";

        private readonly IDialogService dialogService;

        private readonly DelegateCommand selectNextCommand;

        private readonly DelegateCommand selectPreviousCommand;

        private readonly string snapshotFileName;

        private readonly DelegateCommand updateCommand;

        private readonly string updateExchangeInstallerName;

        private readonly string updateExchangeInstallerPath;

        private readonly string updateExchangeTemp;

        private readonly string updateZipChecksumFileName;

        private int currentUpdateIndex;

        private bool isCurrentOperationValid;

        private bool isUpdate;

        private string restoreInfo;

        private InstallerInfo selectedUpdate;

        private ObservableCollection<InstallerInfo> updates = new ObservableCollection<InstallerInfo>();

        private string updatesInfo;

        #endregion

        #region Constructors

        public UpdateStep2ViewModel(IDialogService dialogService)
            : base(PresentationMode.Operator)
        {
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            this.updateExchangeTemp = ConfigurationManager.AppSettings.GetUpdateExchangeTemp();
            this.updateExchangeInstallerPath = ConfigurationManager.AppSettings.GetUpdateExchangeInstallerPath();
            this.updateExchangeInstallerName = ConfigurationManager.AppSettings.GetUpdateExchangeInstallerName();
            this.updateZipChecksumFileName = ConfigurationManager.AppSettings.GetUpdateZipChecksumFileName();
            this.snapshotFileName = ConfigurationManager.AppSettings.GetInstallerSnapshotFileName();

            this.selectNextCommand = new DelegateCommand(() => this.ChangeSelectedUpdate(false), this.CanSelectPrevious);
            this.selectPreviousCommand = new DelegateCommand(() => this.ChangeSelectedUpdate(true), this.CanSelectNext);
            this.updateCommand = new DelegateCommand(async () => await this.UpdateAsync(), this.CanUpdate);
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsUpdate
        {
            get => this.isUpdate;
            set => this.SetProperty(ref this.isUpdate, value);
        }

        public string RestoreInfo
        {
            get => this.restoreInfo;
            set => this.SetProperty(ref this.restoreInfo, value);
        }

        public InstallerInfo SelectedUpdate
        {
            get => this.selectedUpdate;
            set => this.SetProperty(ref this.selectedUpdate, value);
        }

        public ICommand SelectNextCommand => this.selectNextCommand;

        public ICommand SelectPreviousCommand => this.selectPreviousCommand;

        public ICommand UpdateCommand => this.updateCommand;

        public ObservableCollection<InstallerInfo> Updates => this.updates;

        public string UpdatesInfo => this.updatesInfo;

        #endregion

        #region Methods

        public void ChangeSelectedUpdate(bool isUp)
        {
            if (this.updates is null)
            {
                return;
            }

            if (this.updates.Any())
            {
                var newIndex = isUp ? this.currentUpdateIndex - 1 : this.currentUpdateIndex + 1;

                this.currentUpdateIndex = Math.Max(0, Math.Min(newIndex, this.updates.Count - 1));
            }

            this.SelectUpdate();
        }

        public override async Task OnAppearedAsync()
        {
            this.updatesInfo = null;

            await this.DataCheck();

            this.ClearNotifications();

            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;
        }

        public void RaisePropertyChanged()
        {
            this.RaisePropertyChanged(nameof(this.IsUpdate));
            this.RaisePropertyChanged(nameof(this.UpdatesInfo));
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.selectPreviousCommand?.RaiseCanExecuteChanged();
            this.selectNextCommand?.RaiseCanExecuteChanged();
            this.updateCommand?.RaiseCanExecuteChanged();
        }

        private void AppendLine(string log)
        {
            this.updatesInfo += $"{log}{Environment.NewLine}";
            this.Logger.Debug(log);

            this.RaisePropertyChanged(nameof(this.UpdatesInfo));
        }

        private async Task ApplyRestoreAsnc(string restoreFilePath)
        {
            this.ClearNotifications();

            this.RestoreInfo = Localized.Get("InstallationApp.RestoreInProgress");

            this.isCurrentOperationValid = true;
            this.IsEnabled = false;
            this.updatesInfo = string.Empty;

            await this.ClearTempFolderAsync();

            await this.ExtractZipInFolderAsync(restoreFilePath);
            await this.DeleteSnapshotFileAsync();
            await this.StartRestoreAppAsync();

            this.IsEnabled = true;
            this.RaisePropertyChanged();

            if (this.isCurrentOperationValid)
            {
                this.ShowNotification(Localized.Get("InstallationApp.UpdateSuccessfullyCompleted"), Services.Models.NotificationSeverity.Success);
            }
            else
            {
                this.RestoreInfo = Localized.Get("InstallationApp.RestoreEndedWithErrors");
                this.ShowNotification(Localized.Get("InstallationApp.ErrorOnInitialPhaseUpdate"), Services.Models.NotificationSeverity.Error);
            }
        }

        private async Task ApplyUpdatesync()
        {
            if (!this.IsEnabled)
            {
                return;
            }

            this.ClearNotifications();

            this.isCurrentOperationValid = true;
            this.IsEnabled = false;
            this.updatesInfo = string.Empty;

            await this.ClearTempFolderAsync();
            await this.ExtractZipInFolderAsync(this.selectedUpdate.FileName);
            await this.CheckIntegrityOnFilesAsync();
            await this.DeleteSnapshotFileAsync();
            await this.StartInstallerAppAsync();

            this.IsEnabled = true;
            this.RaisePropertyChanged();

            if (this.isCurrentOperationValid)
            {
                this.ShowNotification(Localized.Get("InstallationApp.UpdateSuccessfullyCompleted"), Services.Models.NotificationSeverity.Success);
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                this.ShowNotification(Localized.Get("InstallationApp.ErrorOnInitialPhaseUpdate"), Services.Models.NotificationSeverity.Error);
            }
        }

        private bool CanSelectNext() => this.currentUpdateIndex > 0;

        private bool CanSelectPrevious() => this.currentUpdateIndex < this.updates.Count - 1;

        private bool CanUpdate() =>
            this.selectedUpdate != null
            &&
            this.IsEnabled;

        private async Task CheckIntegrityOnFilesAsync()
        {
            await Task.Run(() =>
            {
                if (!this.isCurrentOperationValid)
                {
                    return;
                }

                try
                {
                    this.isCurrentOperationValid = true;

                    var filePath = $"{this.updateExchangeTemp}{Path.DirectorySeparatorChar}{this.updateZipChecksumFileName}{CSVEXTENSION}";

                    this.AppendLine(string.Format(Localized.Get("InstallationApp.StartChecksum"), filePath));

                    var csvFilesToCheck = File.ReadLines(filePath).Select(a => a.Split(';').First())?.Skip(1);

                    if (csvFilesToCheck is null)
                    {
                        this.isCurrentOperationValid = false;
                        this.AppendLine(string.Format(Localized.Get("InstallationApp.ErrorChecksumNotFound"), filePath));
                        return;
                    }

                    foreach (var csvFileCheck in csvFilesToCheck)
                    {
                        var fileCheck = csvFileCheck.Split(',');
                        var fileName = fileCheck[0].Trim('"').TrimStart('.');
                        var absoluteFilePath = this.updateExchangeTemp + fileName;
                        var checksumFilePath = fileCheck[1].Trim('"');
                        if (this.GeMD5FromFile(absoluteFilePath) != checksumFilePath)
                        {
                            this.AppendLine(Localized.Get("InstallationApp.OperationAborted"));
                            this.AppendLine(string.Format(Localized.Get("InstallationApp.ErrorCheckFile"), fileName));
                            this.isCurrentOperationValid = false;
                            break;
                        }
                    }

                    if (this.isCurrentOperationValid)
                    {
                        this.AppendLine(Localized.Get("InstallationApp.ChecksumCompleted"));
                    }
                }
                catch (Exception ex)
                {
                    this.isCurrentOperationValid = false;
                    this.AppendLine(Localized.Get("InstallationApp.ErrorChecksum"));
                    var errMsg = (ex.InnerException is null) ? ex.Message : ex.InnerException.Message;
                    this.AppendLine(errMsg);
                    this.ShowNotification(ex);
                }
            }
            );
        }

        private async Task ClearTempFolderAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    if (Directory.Exists(this.updateExchangeTemp))
                    {
                        this.AppendLine(string.Format(Localized.Get("InstallationApp.ClearTempFolder"), this.updateExchangeTemp));
                        Directory.Delete(this.updateExchangeTemp, true);
                    }
                }
                catch (Exception ex)
                {
                    this.isCurrentOperationValid = false;
                    this.AppendLine(Localized.Get("InstallationApp.ErrorClearTempFolder"));
                    var errMsg = (ex.InnerException is null) ? ex.Message : ex.InnerException.Message;
                    this.AppendLine(errMsg);
                }
            });
        }

        private async Task DataCheck()
        {
            if (this.Data is IEnumerable<InstallerInfo> installPackages)
            {
                this.IsUpdate = true;

                this.updates.Clear();
                installPackages.ForEach(u => this.updates.Add(u));

                var lastUpdateId = this.selectedUpdate?.Id;

                this.RaisePropertyChanged(nameof(this.Updates));

                this.SetCurrentIndex(lastUpdateId);

                this.SelectUpdate();
            }

            if (this.Data is string restoreFilePath)
            {
                this.IsUpdate = false;
                this.updates.Clear();
                await this.ApplyRestoreAsnc(restoreFilePath);
            }
        }

        private async Task DeleteSnapshotFileAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    var snapshtoFilePath = Path.Combine(
                        this.updateExchangeTemp,
                        this.updateExchangeInstallerPath,
                        this.snapshotFileName);

                    if (File.Exists(snapshtoFilePath))
                    {
                        this.AppendLine(string.Format(Localized.Get("InstallationApp.DeleteSnapshotFile"), snapshtoFilePath));
                        Directory.Delete(snapshtoFilePath, true);
                    }
                }
                catch (Exception ex)
                {
                    this.isCurrentOperationValid = false;
                    this.AppendLine(Localized.Get("InstallationApp.ErrorDeleteSnapshotFile"));
                    var errMsg = (ex.InnerException is null) ? ex.Message : ex.InnerException.Message;
                    this.AppendLine(errMsg);
                }
            });
        }

        private async Task ExtractZipInFolderAsync(string fileName)
        {
            await Task.Run(() =>
            {
                if (!this.isCurrentOperationValid)
                {
                    return;
                }

                this.isCurrentOperationValid = true;

                try
                {
                    this.AppendLine(string.Format(Localized.Get("InstallationApp.StartExtractingFiles"), fileName));
                    ZipFile.ExtractToDirectory(fileName, this.updateExchangeTemp);
                    this.AppendLine(Localized.Get("InstallationApp.ExtractingFilesCompleted"));
                }
                catch (Exception ex)
                {
                    this.isCurrentOperationValid = false;
                    this.AppendLine(Localized.Get("InstallationApp.ErrorExtractingFiles"));
                    var errMsg = (ex.InnerException is null) ? ex.Message : ex.InnerException.Message;
                    this.AppendLine(errMsg);
                }
            });
        }

        private string GeMD5FromFile(string fileName)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    var md5Hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(md5Hash).Replace("-", string.Empty);
                }
            }
        }

        private void SelectUpdate()
        {
            if (this.updates.Any())
            {
                this.SelectedUpdate = this.updates.ElementAt(this.currentUpdateIndex);
            }
            else
            {
                this.SelectedUpdate = null;
            }

            this.RaiseCanExecuteChanged();
        }

        private void SetCurrentIndex(int? missionId)
        {
            if (missionId.HasValue
                &&
                this.updates.FirstOrDefault(l => l.Id == missionId.Value) is InstallerInfo installerFound)
            {
                this.currentUpdateIndex = this.updates.IndexOf(installerFound);
            }
            else
            {
                this.currentUpdateIndex = 0;
            }
        }

        private void StartInstallerApp(string parameter)
        {
            if (!this.isCurrentOperationValid)
            {
                return;
            }

            try
            {
                this.isCurrentOperationValid = true;

                var installerFilePath = Path.Combine(
                    this.updateExchangeTemp,
                    this.updateExchangeInstallerPath,
                    this.updateExchangeInstallerName);

                this.AppendLine(string.Format(Localized.Get("InstallationApp.StartingApplication"), installerFilePath));

                using (var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = installerFilePath,
                        Arguments = parameter
                    }
                })
                {
                    var success = process.Start();

                    if (success)
                    {
                        this.AppendLine(Localized.Get("InstallationApp.InstallerAppStartedSuccessfully"));
                    }
                    else
                    {
                        this.AppendLine(Localized.Get("InstallationApp.ErrorExecutingInstallerApp"));
                        this.isCurrentOperationValid = false;
                    }
                }
            }
            catch (Exception ex)
            {
                this.isCurrentOperationValid = false;
                this.AppendLine(Localized.Get("InstallationApp.ErrorOnStarting"));
                var errMsg = (ex.InnerException is null) ? ex.Message : ex.InnerException.Message;
                this.AppendLine(errMsg);
                this.ShowNotification(ex);
            }
        }

        private async Task StartInstallerAppAsync()
        {
            await Task.Run(() => this.StartInstallerApp(UPDATEARG));
        }

        private async Task StartRestoreAppAsync()
        {
            await Task.Run(() => this.StartInstallerApp(RESTOREARG));
        }

        private async Task UpdateAsync()
        {
            var msg = string.Format(Localized.Get("InstallationApp.ConfirmUpdateVer"), Environment.NewLine, this.selectedUpdate.ProductVersion);
            var messageBoxResult = this.dialogService.ShowMessage(msg, Localized.Get("InstallationApp.ConfirmUpdate"), DialogType.Question, DialogButtons.YesNo);
            if (messageBoxResult is DialogResult.Yes)
            {
                try
                {
                    this.IsBackNavigationAllowed = false;

                    this.IsWaitingForResponse = true;

                    await this.ApplyUpdatesync();

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
