﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Newtonsoft.Json.Linq;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    public class ParametersImportViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private readonly IUsbWatcherService usbWatcher;

        private IEnumerable<FileInfo> configurationFiles = Array.Empty<FileInfo>();

        private object importingConfiguration = null;

        private bool isBusy = false;

        private DelegateCommand saveCommand = null;

        private VertimagConfiguration selectedConfiguration = null;

        private FileInfo selectedFile = null;

        #endregion

        #region Constructors

        public ParametersImportViewModel(
            IMachineConfigurationWebService machineConfigurationWebService,
            IUsbWatcherService usbWatcher)
            : base(PresentationMode.Installer)
        {
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
            this.usbWatcher = usbWatcher;
        }

        #endregion

        #region Properties

        public IEnumerable<FileInfo> ConfigurationFiles => this.configurationFiles;

        public override EnableMask EnableMask => EnableMask.Any;

        /// <summary>
        /// Gets or sets the configuration ready to be imported, if any.
        /// </summary>
        public object ImportingConfiguration
        {
            get => this.importingConfiguration;
            set
            {
                if (this.SetProperty(ref this.importingConfiguration, value))
                {
                    if (this.importingConfiguration != null)
                    {
                        this.ShowNotification(string.Format(Resources.Localized.Get("InstallationApp.RestorePossibleFromFile"), this.selectedFile.Name));
                    }
                    else if (this.selectedConfiguration != null)
                    {
                        this.ShowNotification(string.Format(Resources.Localized.Get("InstallationApp.RestoreNotPossibleFromFile"), this.selectedFile.Name), Services.Models.NotificationSeverity.Warning);
                    }
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
                    this.saveCommand?.RaiseCanExecuteChanged();
                    this.IsBackNavigationAllowed = !this.isBusy;
                }
            }
        }

        public ICommand RestoreCommand =>
                    this.saveCommand
                    ??
                    (this.saveCommand = new DelegateCommand(
                    async () => await this.RestoreAsync(), this.CanRestore));

        /// <summary>
        /// Gets or sets the configuration stored in the current <see cref="SelectedFile"/>, if any.
        /// </summary>
        public VertimagConfiguration SelectedConfiguration
        {
            get => this.selectedConfiguration;
            set => this.SetProperty(ref this.selectedConfiguration, value);
        }

        public FileInfo SelectedFile
        {
            get => this.selectedFile;
            set
            {
                var old = this.selectedFile;
                if (this.SetProperty(ref this.selectedFile, value))
                {
                    this.OnSelectedFileChanged(old, value);
                }
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.usbWatcher.DrivesChanged -= this.UsbWatcher_DrivesChange;
            this.usbWatcher.Disable();

            base.Disappear();
        }

        public override Task OnAppearedAsync()
        {
            // reset selected file to null, just in case
            this.SelectedFile = null;

            this.usbWatcher.DrivesChanged += this.UsbWatcher_DrivesChange;
            this.usbWatcher.Enable();

            this.FindConfigurationFiles();

            return base.OnAppearedAsync();
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);
            if (args?.PropertyName == nameof(this.IsMoving) || args?.PropertyName == nameof(this.ImportingConfiguration))
            {
                this.saveCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool CanRestore()
        {
            return
                !this.IsBusy
                &&
                !this.IsMoving
                &&
                this.ImportingConfiguration != null;
        }

        private void FindConfigurationFiles()
        {
            this.IsBusy = true;
            var usb = this.usbWatcher;
            IEnumerable<FileInfo> configurationFiles = null;

            configurationFiles = this.configurationFiles = usb.Drives.FindConfigurationFiles();

            this.RaisePropertyChanged(nameof(this.ConfigurationFiles));
            if (!configurationFiles.Any())
            {
                this.ShowNotification(Resources.Localized.Get("InstallationApp.NoDevicesAvailableAnymore"), Services.Models.NotificationSeverity.Warning);
                this.NavigationService.GoBackSafelyAsync();
            }
            this.IsBusy = false;
        }

        private void OnSelectedFileChanged(FileInfo _, FileInfo file)
        {
            VertimagConfiguration config = null;
            if (file != null)
            {
                try
                {
                    // merge with an empty configuration (to avoid JsonPropertyAttribute idiosyncrasy)
                    var json = File.ReadAllText(file.FullName);
                    var source = new VertimagConfiguration
                    {
                        Machine = new Machine(),
                    }.ExtendWith(JObject.Parse(json));

                    config = Newtonsoft.Json.JsonConvert.DeserializeObject<VertimagConfiguration>(source.ToString(),
                        new Newtonsoft.Json.JsonConverter[]
                        {
                            new CommonUtils.Converters.IPAddressConverter(),
                            new Newtonsoft.Json.Converters.StringEnumConverter(),
                        });

                    //config = VertimagConfiguration.FromJson(source.ToString());
                }
                catch (Exception exc)
                {
                    this.ShowNotification(exc);
                }
            }
            this.selectedConfiguration = config;
            this.RaisePropertyChanged(nameof(this.SelectedConfiguration));

            this.saveCommand?.RaiseCanExecuteChanged();
        }

        private async Task RestoreAsync()
        {
            try
            {
                this.IsBusy = true;

                // fetch the very last version
                var source = await this.machineConfigurationWebService.GetAsync();

                // merge and save
                var result = source.ExtendWith(this.ImportingConfiguration);
                var target = VertimagConfiguration.FromJson(result.ToString());

                await this.machineConfigurationWebService.SetAsync(target);
                this.SelectedFile = null;
                this.ShowNotification(Resources.Localized.Get("InstallationApp.RestoreSuccessful"), Services.Models.NotificationSeverity.Success);
            }
            catch (Exception exc)
            {
                this.ShowNotification(exc);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private void UsbWatcher_DrivesChange(object sender, DrivesChangedEventArgs e)
        {
            this.FindConfigurationFiles();
        }

        #endregion
    }
}
