using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services.IO;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    public class BaseUpdateViewModel : BaseMainViewModel
    {
        #region Fields

        private const string DEFAULTEXTENSION = "*.exe";

        private const string DEFAULTMANIFESTFILE = "app.manifest";

        private const string DEFAULTMANIFESTFILENODE = "assemblyIdentity";

        private const string DEFAULTMANIFESTFILENODENAME = "name";

        private const string DEFAULTMANIFESTFILENODEVERSION = "version";

        private readonly string configurationUpdateRepositoryPath;

        private readonly EventHandler<DrivesChangeEventArgs> drivesChangeEventHandler;

        private readonly IList<InstallerInfo> installations = new List<InstallerInfo>();

        private readonly UsbWatcherService usbWatcher;

        private bool isBusy;

        private bool isDeviceReady;

        private bool isInstallationReady;

        private bool isRefresh;

        private bool isSystemConfigurationPathReady;

        private IEnumerable<string> zipFilesToCheck;

        #endregion

        #region Constructors

        public BaseUpdateViewModel(UsbWatcherService usb)
            : base(Services.PresentationMode.Installer)
        {
            this.configurationUpdateRepositoryPath = ConfigurationManager.AppSettings.GetUpdateRepositoryPath();
            this.usbWatcher = usb;
            this.drivesChangeEventHandler = new EventHandler<DrivesChangeEventArgs>(async (sender, e) => await this.UsbWatcher_DrivesChangeAsync(sender, e));
        }

        #endregion

        #region Properties

        public string DeviceInfo
        {
            get
            {
                if (!this.isDeviceReady)
                {
                    return Localized.Get("InstallationApp.NoRemovableDevicesFound");
                }

                return Localized.Get("InstallationApp.RemovableDevicesFound");
            }
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public string InstallationFilesInfo
        {
            get
            {
                if (!this.IsInstallationReady)
                {
                    return "-";
                }

                if (this.installations.Count == 0)
                {
                    return Localized.Get("InstallationApp.NoInstallationFileFound");
                }

                if (this.installations.Count == 1)
                {
                    return Localized.Get("InstallationApp.OneInstallationFileFound");
                }

                return string.Format(Localized.Get("InstallationApp.InstallationFileFound"), this.installations.Count);
            }
        }

        public IList<InstallerInfo> Installations => new List<InstallerInfo>(this.installations);

        public bool IsBusy
        {
            get => this.isBusy;
            set
            {
                if (this.SetProperty(ref this.isBusy, value))
                {
                    this.IsEnabled = !this.isBusy;
                    this.RaisePropertyChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        public bool IsDeviceReady
        {
            get => this.isDeviceReady;
            set => this.SetProperty(ref this.isDeviceReady, value);
        }

        public bool IsInstallationReady
        {
            get => this.isInstallationReady;
            set => this.SetProperty(ref this.isInstallationReady, value);
        }

        public bool IsInstallationReadyFromSystemConfiguration => this.isSystemConfigurationPathReady && this.IsInstallationReady;

        public string SharedFolderInfo
        {
            get
            {
                var pathNotDFound = Localized.Get("InstallationApp.SystemPathNotFound");
                if (!this.isSystemConfigurationPathReady)
                {
                    return string.Format(pathNotDFound, this.configurationUpdateRepositoryPath);
                }

                return string.Format(pathNotDFound, this.configurationUpdateRepositoryPath);
            }
        }

        public string SystemInfo => Assembly.GetEntryAssembly().GetName()?.Version?.ToString();

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.usbWatcher.DrivesChange -= this.drivesChangeEventHandler;
            this.usbWatcher.Dispose();

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            try
            {
                this.usbWatcher.DrivesChange += this.drivesChangeEventHandler;
                this.usbWatcher.Start();

                this.ClearNotifications();

                await base.OnAppearedAsync();
            }
            catch (Exception ex)
            {
                this.IsBusy = false;
                this.ShowNotification(ex);
            }
        }

        public virtual void RaisePropertyChanged()
        {
            this.RaisePropertyChanged(nameof(this.SharedFolderInfo));
            this.RaisePropertyChanged(nameof(this.DeviceInfo));
            this.RaisePropertyChanged(nameof(this.InstallationFilesInfo));
        }

        private void CheckForValidZipFiles(IEnumerable<string> zipFiles, bool isRemotePath)
        {
            foreach (var zipFile in zipFiles)
            {
                try
                {
                    using (var archive = ZipFile.OpenRead(zipFile))
                    {
                        var installerFileInfo = new InstallerInfo(zipFile, isRemotePath);

                        foreach (var entry in archive.Entries.Where(e => e.FullName.Contains(DEFAULTMANIFESTFILE)))
                        {
                            using (var stream = entry.Open())
                            {
                                var attributes = this.GetFileAttribute(stream);
                                if (!string.IsNullOrEmpty(attributes.assemblyVersion)
                                    &&
                                    !string.IsNullOrEmpty(attributes.assemblyName))
                                {
                                    installerFileInfo.SetAssemblyVersion(attributes.assemblyName, attributes.assemblyVersion);
                                }
                            }
                        }

                        if (installerFileInfo.IsValid)
                        {
                            this.installations.Add(installerFileInfo);
                        }
                    }
                }
                catch
                {
                    // Not supported file
                }
            }
        }

        private void CheckPath(IEnumerable<string> zipFiles, bool isRemotePath)
        {
            this.CheckForValidZipFiles(zipFiles, isRemotePath);
            this.IsInstallationReady = this.installations.Count > 0;
        }

        private (string assemblyVersion, string assemblyName) GetFileAttribute(Stream stream)
        {
            string assemblyVersion = null;
            string assemblyName = null;

            using (var xmlReader = XmlReader.Create(stream))
            {
                while (xmlReader.Read())
                {
                    if ((xmlReader.NodeType == XmlNodeType.Element)
                        &&
                        (xmlReader.Name == DEFAULTMANIFESTFILENODE))
                    {
                        if (xmlReader.HasAttributes)
                        {
                            assemblyVersion = xmlReader.GetAttribute(DEFAULTMANIFESTFILENODEVERSION);
                            assemblyName = xmlReader.GetAttribute(DEFAULTMANIFESTFILENODENAME);
                        }
                    }
                }
            }

            return (assemblyVersion, assemblyName);
        }

        private void Refresh()
        {
            try
            {
                if (this.isRefresh)
                {
                    return;
                }

                this.IsBusy = true;

                this.isRefresh = true;

                this.installations.Clear();

                this.RefreshDefaultPath();

                this.RefreshDevicesStatus();

                this.RaiseCanExecuteChanged();
                this.RaisePropertyChanged();

                this.isRefresh = false;
            }
            catch (Exception ex)
            {
                this.isRefresh = false;
                this.ShowNotification(ex);
            }

            this.IsBusy = false;
        }

        private async Task RefreshAsync()
        {
            await Task.Run(() => this.Refresh());
        }

        private void RefreshDefaultPath()
        {
            if (!string.IsNullOrEmpty(this.configurationUpdateRepositoryPath)
                &&
                !Directory.Exists(this.configurationUpdateRepositoryPath))
            {
                this.isSystemConfigurationPathReady = false;
                return;
            }

            this.isSystemConfigurationPathReady = true;

            var repositoryZipFiles = Directory.EnumerateFiles(this.configurationUpdateRepositoryPath, DEFAULTEXTENSION, SearchOption.TopDirectoryOnly);

            this.CheckPath(repositoryZipFiles, true);
        }

        private void RefreshDevicesStatus()
        {
            if (this.zipFilesToCheck?.Any() == true)
            {
                this.CheckPath(this.zipFilesToCheck, false);
            }
            else
            {
                this.zipFilesToCheck = null;
                this.IsInstallationReady = false;
                this.IsDeviceReady = false;
            }
        }

        private async Task UsbWatcher_DrivesChangeAsync(object sender, DrivesChangeEventArgs e)
        {
            if (e.Attached?.Any() == true)
            {
                var drives = ((UsbWatcherService)sender).Drives;
                this.zipFilesToCheck = drives.FindFiles(DEFAULTEXTENSION).ToList();
            }
            else
            {
                this.zipFilesToCheck = null;
            }

            await this.RefreshAsync();
        }

        #endregion
    }
}
