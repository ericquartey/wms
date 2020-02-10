using System;
using System.Collections.Generic;
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
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    public class UpdateStep2ViewModel : BaseMainViewModel
    {
        #region Fields

        private const string CSVEXTENSION = ".csv";

        private readonly IDialogService dialogService;

        private readonly string updateExchangeInstallerName;

        private readonly string updateExchangeInstallerPath;

        private readonly string updateExchangeTemp;

        private readonly IList<InstallerInfo> updates = new List<InstallerInfo>();

        private readonly string updateZipChecksumFileName;

        private int currentUpdateIndex;

        private DelegateCommand downCommand;

        private bool isCurrentOperationValid;

        private InstallerInfo selectedUpdate;

        private DelegateCommand upCommand;

        private DelegateCommand updateCommand;

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
        }

        #endregion

        #region Properties

        public ICommand DownCommand =>
            this.downCommand
            ??
            (this.downCommand = new DelegateCommand(() => this.ChangeSelectedUpdate(false), this.CanDown));

        public override EnableMask EnableMask => EnableMask.Any;

        public InstallerInfo SelectedUpdate
        {
            get =>
                this.selectedUpdate;
            set => this.SetProperty(ref this.selectedUpdate, value);
        }

        public ICommand UpCommand =>
            this.upCommand
            ??
            (this.upCommand = new DelegateCommand(() => this.ChangeSelectedUpdate(true), this.CanUp));

        public ICommand UpdateCommand =>
             this.updateCommand
             ??
             (this.updateCommand = new DelegateCommand(async () => await this.UpdateAsync(), this.CanUpdate));

        public IList<InstallerInfo> Updates => new List<InstallerInfo>(this.updates);

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

            this.SelectLoadingUnit();
        }

        public override async Task OnAppearedAsync()
        {
            this.updatesInfo = null;

            this.DataCheck();

            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;
        }

        public void RaisePropertyChanged()
        {
            this.RaisePropertyChanged(nameof(this.UpdatesInfo));
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.upCommand?.RaiseCanExecuteChanged();
            this.downCommand?.RaiseCanExecuteChanged();
            this.updateCommand?.RaiseCanExecuteChanged();
        }

        private void AppendLine(string log)
        {
            this.updatesInfo += $"{log}{Environment.NewLine}";

            this.RaisePropertyChanged(nameof(this.UpdatesInfo));
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
            await this.ExtractZipInFolderAsync();
            await this.CheckIntegrityOnFilesAsync();
            await this.StartInstallerAppAsync();

            this.IsEnabled = true;
            this.RaisePropertyChanged();

            if (this.isCurrentOperationValid)
            {
                this.ShowNotification(InstallationApp.UpdateSuccessfullyCompleted, Services.Models.NotificationSeverity.Success);
            }
            else
            {
                this.ShowNotification(InstallationApp.ErrorOnInitialPhaseUpdate, Services.Models.NotificationSeverity.Error);
            }
        }

        private bool CanDown()
        {
            return
              this.currentUpdateIndex < this.updates.Count - 1;
        }

        private bool CanUp()
        {
            return
                this.currentUpdateIndex > 0;
        }

        private bool CanUpdate()
        {
            return this.selectedUpdate != null
                   &&
                   this.IsEnabled;
        }

        private void CheckIntegrityOnFiles()
        {
            if (!this.isCurrentOperationValid)
            {
                return;
            }

            try
            {
                this.isCurrentOperationValid = true;

                var filePath = $"{this.updateExchangeTemp}{Path.DirectorySeparatorChar}{this.updateZipChecksumFileName}{CSVEXTENSION}";

                this.AppendLine($"Start checking files checksum '{filePath}'.");

                var csvFilesToCheck = File.ReadLines(filePath).Select(a => a.Split(';').First())?.Skip(1);

                if (csvFilesToCheck is null)
                {
                    this.isCurrentOperationValid = false;
                    this.AppendLine($"Error on checking file '{filePath}'. No checksums found.");
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
                        this.AppendLine($"Operation aborted.");
                        this.AppendLine($"Error on checking file '{fileName}'");
                        this.isCurrentOperationValid = false;
                        break;
                    }
                }

                if (this.isCurrentOperationValid)
                {
                    this.AppendLine("Checksum files check successfully completed.");
                }
            }
            catch (Exception ex)
            {
                this.isCurrentOperationValid = false;
                this.AppendLine($"Error on checking files checksum:");
                var errMsg = (ex.InnerException is null) ? ex.Message : ex.InnerException.Message;
                this.AppendLine(errMsg);
                this.ShowNotification(ex);
            }
        }

        private async Task CheckIntegrityOnFilesAsync()
        {
            await Task.Run(() => this.CheckIntegrityOnFiles());
        }

        private async Task ClearTempFolderAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    this.AppendLine($"Clear temp folder '{this.updateExchangeTemp}'.");
                    if (Directory.Exists(this.updateExchangeTemp))
                    {
                        Directory.Delete(this.updateExchangeTemp, true);
                    }
                }
                catch (Exception ex)
                {
                    this.isCurrentOperationValid = false;
                    this.AppendLine($"Error on clear temp folder:");
                    var errMsg = (ex.InnerException is null) ? ex.Message : ex.InnerException.Message;
                    this.AppendLine(errMsg);
                }
            });
        }

        private void DataCheck()
        {
            if (this.Data is IList<InstallerInfo> updatesFound)
            {
                this.updates.Clear();
                updatesFound.ForEach(u => this.updates.Add(u));
                var lastUpdateId = this.selectedUpdate?.Id;

                this.RaisePropertyChanged(nameof(this.Updates));

                this.SetCurrentIndex(lastUpdateId);

                this.SelectLoadingUnit();
            }
        }

        private async Task ExtractZipInFolderAsync()
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
                    this.AppendLine($"Start excracting files from '{this.selectedUpdate.FileName}'.");
                    ZipFile.ExtractToDirectory(this.selectedUpdate.FileName, this.updateExchangeTemp);
                    this.AppendLine("Extraction successfullly completed.");
                }
                catch (Exception ex)
                {
                    this.isCurrentOperationValid = false;
                    this.AppendLine($"Error excracting files:");
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

        private void SelectLoadingUnit()
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

        private void StartInstallerApp()
        {
            if (!this.isCurrentOperationValid)
            {
                return;
            }

            try
            {
                this.isCurrentOperationValid = true;

                var installerFilePath = $"{this.updateExchangeTemp}\\{this.updateExchangeInstallerPath}\\{this.updateExchangeInstallerName}";

                this.AppendLine($"Starting application '{installerFilePath}'.");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = installerFilePath
                    }
                };

                process.Start();
                process.WaitForExit();
                var exitCode = process.ExitCode;

                if (exitCode == 0)
                {
                    this.AppendLine(InstallationApp.InstallerAppStartedSuccessfully);
                }
                else
                {
                    this.AppendLine(InstallationApp.ErrorExecutingInstallerApp);
                    this.isCurrentOperationValid = false;
                }
            }
            catch (Exception ex)
            {
                this.isCurrentOperationValid = false;
                this.AppendLine($"Error on starting:");
                var errMsg = (ex.InnerException is null) ? ex.Message : ex.InnerException.Message;
                this.AppendLine(errMsg);
                this.ShowNotification(ex);
            }
        }

        private async Task StartInstallerAppAsync()
        {
            await Task.Run(() => this.StartInstallerApp());
        }

        private async Task UpdateAsync()
        {
            var msg = string.Format(InstallationApp.ConfirmUpdateVer, Environment.NewLine, this.selectedUpdate.ProductVersion);
            var messageBoxResult = this.dialogService.ShowMessage(msg, InstallationApp.ConfirmUpdate, DialogType.Question, DialogButtons.YesNo);
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
