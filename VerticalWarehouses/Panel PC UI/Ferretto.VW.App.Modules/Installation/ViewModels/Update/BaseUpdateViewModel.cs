using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Ferretto.VW.App.Resources;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Models;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    public class BaseUpdateViewModel : BaseMainViewModel
    {
        #region Fields

        private const string DEFAULTDEVICENAME = "F";

        private const string DEFAULTEXTENSION = "*.exe";

        private const string DEFAULTMANIFESTFILE = "app.manifest";

        private const string DEFAULTMANIFESTFILENODE = "assemblyIdentity";

        private const string DEFAULTMANIFESTFILENODENAME = "name";

        private const string DEFAULTMANIFESTFILENODEVERSION = "version";

        private const string DEVICE = @"{0}:\";

        private const int SECSUPDATESINTERVAL = 3;

        private readonly string configurationUpdateRepositoryPath;

        private readonly IList<InstallerInfo> installations = new List<InstallerInfo>();

        private bool isBusy;

        private bool isDeviceReady;

        private bool isInstallationReady;

        private bool isRefresh;

        private bool isSystemConfigurationPathReady;

        private string path;

        private DispatcherTimer updateTimer;

        #endregion

        #region Constructors

        public BaseUpdateViewModel()
            : base(Services.PresentationMode.Installer)
        {
            this.configurationUpdateRepositoryPath = ConfigurationManager.AppSettings.GetUpdateRepositoryPath();
        }

        #endregion

        #region Properties

        public string DeviceInfo
        {
            get
            {
                if (!this.isDeviceReady)
                {
                    return $"No device '{DEFAULTDEVICENAME}' found";
                }

                return $"Device '{DEFAULTDEVICENAME}' is ready";
            }
        }

        public string DevicePath
        {
            get => this.path;
            set => this.SetProperty(ref this.path, value);
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
                    return InstallationApp.NoInstallationFileFound;
                }

                if (this.installations.Count == 1)
                {
                    return InstallationApp.OneInstallationFileFound;
                }

                return string.Format(InstallationApp.InstallationFileFound, this.installations.Count);
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
                if (!this.isSystemConfigurationPathReady)
                {
                    return $"System path '{this.configurationUpdateRepositoryPath}' not found";
                }

                return $"System path '{this.configurationUpdateRepositoryPath}' is valid";
            }
        }

        public string SystemInfo => Assembly.GetEntryAssembly().GetName()?.Version?.ToString();

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.updateTimer?.Stop();
            this.updateTimer = null;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBusy = true;

            try
            {
                this.ClearNotifications();

                await base.OnAppearedAsync();

                await this.RefreshAsync();

                this.StartMonitoring();

                this.IsBusy = false;
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
            this.installations.Clear();

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

        private void CheckPath(string path, bool isRemotePath)
        {
            var zipFiles = Directory.EnumerateFiles(path, DEFAULTEXTENSION, SearchOption.TopDirectoryOnly);
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

                this.isRefresh = true;

                this.RefreshDefaultPath();

                //if (!(this.isSystemConfigurationPathReady
                //      &&
                //      this.IsInstallationReady))
                //{
                this.RefreshDevicesStatus();

                //}

                this.RaiseCanExecuteChanged();
                this.RaisePropertyChanged();

                this.isRefresh = false;
            }
            catch (Exception ex)
            {
                this.isRefresh = false;
                this.ShowNotification(ex);
            }
        }

        private async Task RefreshAsync()
        {
            await Task.Run(() => this.Refresh());
        }

        private void RefreshDefaultPath()
        {
            if (!Directory.Exists(this.configurationUpdateRepositoryPath))
            {
                this.isSystemConfigurationPathReady = false;
                return;
            }

            this.isSystemConfigurationPathReady = true;

            this.CheckPath(this.configurationUpdateRepositoryPath, true);
        }

        private void RefreshDevicesStatus()
        {
            var deviceName = string.Format(DEVICE, DEFAULTDEVICENAME);

            if (DriveInfo.GetDrives()
                .Select(d => d.Name)
                .FirstOrDefault(n => n.Equals(deviceName)) is string deviceFound)
            {
                if (!string.IsNullOrEmpty(this.DevicePath)
                    &&
                    this.DevicePath == deviceFound)
                {
                    return;
                }

                this.DevicePath = deviceFound;
                this.IsDeviceReady = true;
                this.CheckPath(this.DevicePath, false);
            }
            else
            {
                this.DevicePath = null;
                this.IsInstallationReady = false;
                this.IsDeviceReady = false;
            }
        }

        private void StartMonitoring()
        {
            this.updateTimer = new System.Windows.Threading.DispatcherTimer();
            this.updateTimer.Tick += async (sender, e) =>
            {
                await this.UpdateTimer_Tick();
            };
            this.updateTimer.Interval = new TimeSpan(0, 0, SECSUPDATESINTERVAL);
            this.updateTimer.Start();
        }

        private async Task UpdateTimer_Tick()
        {
            await this.RefreshAsync();
        }

        #endregion
    }
}
