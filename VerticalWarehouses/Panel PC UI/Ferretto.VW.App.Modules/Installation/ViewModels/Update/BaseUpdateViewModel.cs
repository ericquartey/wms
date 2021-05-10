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
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    public class BaseUpdateViewModel : BaseMainViewModel
    {
        #region Fields

        private const string DEFAULTMANIFESTFILE = "app.manifest";

        private const string DEFAULTMANIFESTFILENODE = "assemblyIdentity";

        private const string DEFAULTMANIFESTFILENODENAME = "name";

        private const string DEFAULTMANIFESTFILENODEVERSION = "version";

        private const string InstallPackageExeExtension = "VMag*.exe";

        private const string InstallPackageZipExtension = "VMag*.zip";

        private readonly IUsbWatcherService usbWatcher;

        private IEnumerable<InstallerInfo> installations = new List<InstallerInfo>();

        private bool isBusy;

        private bool isRepositoryAvailable;

        private bool isScanInProgress;

        private IEnumerable<string> removableDevicePackages;

        private int removableDevicesCount;

        private int removableDevicesPackagesCount;

        private IEnumerable<string> repositoryPackages;

        private int repositoryPackagesCount;

        private IEnumerable<string> repositoryPackagesSecondary;

        private int repositoryPackagesSecondaryCount;

        private string repositoryPath;

        private string repositoryPathSecondary;

        #endregion

        #region Constructors

        public BaseUpdateViewModel(IUsbWatcherService usbWatcher)
            : base(PresentationMode.Installer)
        {
            this.usbWatcher = usbWatcher;
        }

        #endregion

        #region Properties

        public string AInfo
        {
            get
            {
                if (!this.IsRepositoryAvailable)
                {
                    return "-";
                }

                if (!this.installations.Any())
                {
                    return Localized.Get("InstallationApp.NoInstallationFileFound");
                }

                if (this.installations.Count() == 1)
                {
                    return Localized.Get("InstallationApp.OneInstallationFileFound");
                }

                return string.Format(Localized.Get("InstallationApp.InstallationFileFound"), this.installations.Count());
            }
        }

        public string ApplicationVersion => Assembly.GetEntryAssembly().GetName()?.Version?.ToString();

        public override EnableMask EnableMask => EnableMask.Any;

        public IEnumerable<InstallerInfo> Installations
        {
            get => this.installations;
            set => this.SetProperty(ref this.installations, value, () =>
            {
                this.RepositoryPackagesCount = this.installations.Count(p => !p.IsOnUsb && !p.IsOnMainPc);
                this.RepositoryPackagesSecondaryCount = this.installations.Count(p => !p.IsOnUsb && p.IsOnMainPc);
                this.RemovableDevicesPackagesCount = this.installations.Count(p => p.IsOnUsb);

                this.RaiseCanExecuteChanged();
            });
        }

        public bool IsBay1 => this.MachineService.Bay.Number == MAS.AutomationService.Contracts.BayNumber.BayOne;

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

        public bool IsRepositoryAvailable
        {
            get => this.isRepositoryAvailable;
            set => this.SetProperty(ref this.isRepositoryAvailable, value, () => this.RaisePropertyChanged(nameof(this.RepositoryInfo)));
        }

        public int RemovableDevicesCount
        {
            get => this.removableDevicesCount;
            set => this.SetProperty(ref this.removableDevicesCount, value);
        }

        public int RemovableDevicesPackagesCount
        {
            get => this.removableDevicesPackagesCount;
            set => this.SetProperty(ref this.removableDevicesPackagesCount, value);
        }

        public string RepositoryInfo => this.IsRepositoryAvailable
            ? Localized.Get("InstallationApp.SystemPathAvailable")
            : string.Format(Localized.Get("InstallationApp.SystemPathNotFound"), this.RepositoryPath);

        public int RepositoryPackagesCount
        {
            get => this.repositoryPackagesCount;
            set => this.SetProperty(ref this.repositoryPackagesCount, value);
        }

        public int RepositoryPackagesSecondaryCount
        {
            get => this.repositoryPackagesSecondaryCount;
            set => this.SetProperty(ref this.repositoryPackagesSecondaryCount, value);
        }

        public string RepositoryPath
        {
            get => this.repositoryPath;
            set => this.SetProperty(ref this.repositoryPath, value);
        }

        public string RepositoryPathSecondary
        {
            get => this.repositoryPathSecondary;
            set => this.SetProperty(ref this.repositoryPathSecondary, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.usbWatcher.DrivesChanged -= this.UsbWatcher_DrivesChange;
            this.usbWatcher.Disable();

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.ClearNotifications();

            try
            {
                this.RepositoryPath = ConfigurationManager.AppSettings.GetUpdateRepositoryPath();

                this.RepositoryPathSecondary = ConfigurationManager.AppSettings.GetUpdateRepositoryPathSecondary();

                this.usbWatcher.DrivesChanged += this.UsbWatcher_DrivesChange;
                this.usbWatcher.Enable();

                this.ScanAllPackageSources();

                await base.OnAppearedAsync();
            }
            catch (Exception ex)
            {
                this.IsBusy = false;
                this.ShowNotification(ex);
            }
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

        private void ScanAllPackageSources()
        {
            if (this.isScanInProgress)
            {
                return;
            }

            this.isScanInProgress = true;

            try
            {
                this.IsBusy = true;

                this.repositoryPackages = this.ScanRepository();

                this.repositoryPackagesSecondary = this.ScanSecondaryRepository();

                this.removableDevicePackages = this.ScanRemovableDevices();

                this.Installations = this.SelectValidPackages();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.isScanInProgress = false;
                this.IsBusy = false;
            }
        }

        private IEnumerable<string> ScanRemovableDevices()
        {
            this.RemovableDevicesCount = this.usbWatcher.Drives.Count();
            if (this.RemovableDevicesCount > 0)
            {
                var exePackageFiles = this.usbWatcher.Drives.FindFiles(InstallPackageExeExtension);
                var zipPackageFiles = this.usbWatcher.Drives.FindFiles(InstallPackageZipExtension);

                if (!(zipPackageFiles is null))
                {
                    exePackageFiles = exePackageFiles.Union(zipPackageFiles);
                }

                return exePackageFiles.ToArray();
            }

            return Array.Empty<string>();
        }

        private IEnumerable<string> ScanRepository()
        {
            this.IsRepositoryAvailable = Directory.Exists(this.RepositoryPath);
            if (this.IsRepositoryAvailable)
            {
                var exePackageFiles = Directory.EnumerateFiles(this.RepositoryPath, InstallPackageExeExtension, SearchOption.TopDirectoryOnly);
                var zipPackageFiles = Directory.EnumerateFiles(this.RepositoryPath, InstallPackageZipExtension, SearchOption.TopDirectoryOnly);

                if (!(zipPackageFiles is null))
                {
                    exePackageFiles = exePackageFiles.Union(zipPackageFiles);
                }

                return exePackageFiles.ToArray();
            }

            return Array.Empty<string>();
        }

        private IEnumerable<string> ScanSecondaryRepository()
        {
            this.IsRepositoryAvailable = Directory.Exists(this.RepositoryPathSecondary);
            if (this.IsRepositoryAvailable &&
                !this.IsBay1)
            {
                var exePackageFiles = Directory.EnumerateFiles(this.RepositoryPathSecondary, InstallPackageExeExtension, SearchOption.TopDirectoryOnly);
                var zipPackageFiles = Directory.EnumerateFiles(this.RepositoryPathSecondary, InstallPackageZipExtension, SearchOption.TopDirectoryOnly);

                if (!(zipPackageFiles is null))
                {
                    exePackageFiles = exePackageFiles.Union(zipPackageFiles);
                }

                return exePackageFiles.ToArray();
            }

            return Array.Empty<string>();
        }

        private IEnumerable<InstallerInfo> SelectValidPackages()
        {
            var validPackages = new List<InstallerInfo>();
            var packages = this.removableDevicePackages;

            if (!(this.repositoryPackages is null))
            {
                packages = packages.Union(this.repositoryPackages);
            }

            if (!(this.repositoryPackagesSecondary is null))
            {
                packages = packages.Union(this.repositoryPackagesSecondary);
            }

            foreach (var fileName in packages)
            {
                try
                {
                    using (var archive = ZipFile.OpenRead(fileName))
                    {
                        var isUsb = this.removableDevicePackages.Contains(fileName);
                        var isOnMainPc = this.repositoryPackagesSecondary.Contains(fileName);
                        var installerFileInfo = new InstallerInfo(fileName, isUsb, isOnMainPc);

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
                            validPackages.Add(installerFileInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.Warn(ex);
                }
            }

            return validPackages.ToArray();
        }

        private void UsbWatcher_DrivesChange(object sender, DrivesChangedEventArgs e)
        {
            if (this.isScanInProgress)
            {
                return;
            }

            this.isScanInProgress = true;

            this.IsBusy = true;

            try
            {
                this.removableDevicePackages = this.ScanRemovableDevices();

                this.Installations = this.SelectValidPackages();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
                this.isScanInProgress = false;
            }
        }

        #endregion
    }
}
