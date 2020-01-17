using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;
using Ferretto.VW.App.Controls;
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

        private readonly string configurationUpdatePath;

        private readonly IList<InstallerInfo> installations = new List<InstallerInfo>();

        private bool isBusy;

        private bool isDeviceReady;

        private bool isInstallationReady;

        private bool isSystemConfigurationPathReady;

        private string path;

        private DispatcherTimer updateTimer;

        #endregion

        #region Constructors

        public BaseUpdateViewModel()
            : base(Services.PresentationMode.Installer)
        {
            this.configurationUpdatePath = ConfigurationManager.AppSettings.GetUpdatePath();
        }

        #endregion

        #region Properties

        public string DeviceInfo
        {
            get
            {
                if (!this.isDeviceReady)
                {
                    return "No device found";
                }

                return "Device is ready";
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
                if (!this.isDeviceReady)
                {
                    return "-";
                }

                if (!this.IsInstallationReady)
                {
                    return "No installation files found";
                }

                if (this.installations.Count == 1)
                {
                    return "One installation file found";
                }

                return string.Format("{0} installation files found", this.installations.Count);
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

        public string SystemInfo
        {
            get
            {
                if (!this.isSystemConfigurationPathReady)
                {
                    return $"System path '{this.configurationUpdatePath}' not found";
                }

                return "Valid system path found";
            }
        }

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
            await base.OnAppearedAsync();

            try
            {
                this.IsBusy = true;

                this.Refresh();

                this.StartMonitorDrive();
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

        public virtual void RaisePropertyChanged()
        {
            this.RaisePropertyChanged(nameof(this.SystemInfo));
            this.RaisePropertyChanged(nameof(this.DeviceInfo));
            this.RaisePropertyChanged(nameof(this.InstallationFilesInfo));
        }

        private void CheckForValidZipFiles(IEnumerable<string> zipFiles)
        {
            this.installations.Clear();

            foreach (var zipFile in zipFiles)
            {
                try
                {
                    using (var archive = ZipFile.OpenRead(zipFile))
                    {
                        var installerFileInfo = new InstallerInfo(zipFile);

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
                    // Not suppeorted file
                }
            }
        }

        private void CheckPath(string path)
        {
            var zipFiles = Directory.EnumerateFiles(path, DEFAULTEXTENSION, SearchOption.TopDirectoryOnly);
            this.CheckForValidZipFiles(zipFiles);
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
            this.RefreshDefaultPath();

            if (this.isSystemConfigurationPathReady
                &&
                this.IsInstallationReady)
            {
                return;
            }

            this.RefreshDevicesStatus();
            this.RaisePropertyChanged();
        }

        private void RefreshDefaultPath()
        {
            if (!Directory.Exists(this.configurationUpdatePath))
            {
                this.isSystemConfigurationPathReady = false;
                return;
            }

            this.isSystemConfigurationPathReady = true;

            this.CheckPath(this.configurationUpdatePath);
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
                this.CheckPath(this.DevicePath);
            }
            else
            {
                this.DevicePath = null;
                this.IsInstallationReady = false;
                this.IsDeviceReady = false;
            }
        }

        private void StartMonitorDrive()
        {
            this.updateTimer = new System.Windows.Threading.DispatcherTimer();
            this.updateTimer.Tick += new EventHandler(this.UpdateTimer_Tick);
            this.updateTimer.Interval = new TimeSpan(0, 0, SECSUPDATESINTERVAL);
            this.updateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            this.Refresh();
        }

        #endregion
    }

    public class InstallerInfo
    {
        #region Fields

        private const string ASSEMBLYNAMEPANELINSTALLER = "Ferretto.VW.Installer";

        private const string ASSEMBLYNAMEPANELPC = "Ferretto.VW.App";

        private const string ASSEMBLYNAMESERVICE = "Ferretto.VW.MAS.AutomationService";

        #endregion

        #region Constructors

        public InstallerInfo(string fileName)
        {
            this.FileName = fileName;
        }

        public InstallerInfo(string productVersion, string serviceVersion, string panelPcVersion, string fileName)
        {
            this.ProductVersion = productVersion;
            this.ServiceVersion = serviceVersion;
            this.PanelPcVersion = panelPcVersion;
            this.FileName = fileName;
        }

        #endregion

        #region Properties

        public string FileName { get; private set; }

        public bool IsValid => !string.IsNullOrEmpty(this.FileName) &&
                               !string.IsNullOrEmpty(this.PanelPcVersion) &&
                               //!string.IsNullOrEmpty(this.ProductVersion) &&
                               !string.IsNullOrEmpty(this.ServiceVersion);

        public string PanelPcVersion { get; private set; }

        public string ProductVersion { get; private set; }

        public string ServiceVersion { get; private set; }

        #endregion

        #region Methods

        public void SetAssemblyVersion(string assemblyName, string version)
        {
            if (assemblyName == ASSEMBLYNAMEPANELINSTALLER)
            {
                this.ProductVersion = version;
            }

            if (assemblyName == ASSEMBLYNAMEPANELPC)
            {
                this.PanelPcVersion = version;
            }

            if (assemblyName == ASSEMBLYNAMESERVICE)
            {
                this.ServiceVersion = version;
            }
        }

        public void SetFileName(string fileName)
        {
            this.FileName = fileName;
        }

        #endregion
    }
}
