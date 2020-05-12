using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class BarcodeReaderSettingsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBarcodeReaderService barcodeReaderService;

        private readonly IMachineBaysWebService baysWebService;

        private DelegateCommand configureDeviceCommand;

        private string firmwareVersion;

        private bool isAccessoryEnabled;

        private DateTime? manufactureDate;

        private string modelNumber;

        private string portName;

        private DelegateCommand saveCommand;

        private string serialNumber;

        private bool systemPortsAvailable;

        #endregion

        #region Constructors

        public BarcodeReaderSettingsViewModel(
            IBarcodeReaderService barcodeReaderService,
            IMachineBaysWebService baysWebService)
            : base(PresentationMode.Installer)
        {
            this.barcodeReaderService = barcodeReaderService;
            this.baysWebService = baysWebService;

            this.barcodeReaderService.PortNames.CollectionChanged += this.OnPortNamesChanged;
        }

        #endregion

        #region Properties

        public ICommand ConfigureDeviceCommand =>
          this.configureDeviceCommand
          ??
          (this.configureDeviceCommand = new DelegateCommand(
              this.ConfigureDevice,
              this.CanConfigureDevice));

        public string FirmwareVersion
        {
            get => this.firmwareVersion;
            set => this.SetProperty(ref this.firmwareVersion, value);
        }

        public bool IsAccessoryEnabled
        {
            get => this.isAccessoryEnabled;
            set => this.SetProperty(ref this.isAccessoryEnabled, value, this.RaiseCanExecuteChanged);
        }

        public DateTime? ManufactureDate
        {
            get => this.manufactureDate;
            set => this.SetProperty(ref this.manufactureDate, value);
        }

        public string ModelNumber
        {
            get => this.modelNumber;
            set => this.SetProperty(ref this.modelNumber, value);
        }

        public string PortName
        {
            get => this.portName;
            set => this.SetProperty(ref this.portName, value);
        }

        public IEnumerable<string> PortNames => this.barcodeReaderService.PortNames;

        public ICommand SaveCommand =>
            this.saveCommand
            ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.SaveAsync(),
                this.CanSave));

        public string SerialNumber
        {
            get => this.serialNumber;
            set => this.SetProperty(ref this.serialNumber, value);
        }

        public bool SystemPortsAvailable
        {
            get => this.systemPortsAvailable;
            set => this.SetProperty(ref this.systemPortsAvailable, value);
        }

        #endregion

        #region Methods

        protected override async Task OnDataRefreshAsync()
        {
            await base.OnDataRefreshAsync();

            try
            {
                if (this.Data is BayAccessories bayAccessories)
                {
                    this.IsAccessoryEnabled = bayAccessories.BarcodeReader.IsEnabled;
                    this.PortName = bayAccessories.BarcodeReader.PortName;
                }
                else
                {
                    this.Logger.Warn("Improper parameters were passed to the barcode reader settings page. Leaving the page ...");

                    this.NavigationService.GoBack();
                }

                await this.barcodeReaderService.StartAsync();

                this.FirmwareVersion = this.barcodeReaderService.DeviceInformation.FirmwareVersion;
                this.SerialNumber = this.barcodeReaderService.DeviceInformation.SerialNumber;
                this.ManufactureDate = this.barcodeReaderService.DeviceInformation.ManufactureDate;
                this.ModelNumber = this.barcodeReaderService.DeviceInformation.ModelNumber;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.saveCommand?.RaiseCanExecuteChanged();
        }

        private bool CanConfigureDevice()
        {
            return !this.IsWaitingForResponse;
        }

        private bool CanSave()
        {
            return !this.IsWaitingForResponse;
        }

        private void ConfigureDevice()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Accessories.BarcodeReaderConfig);
        }

        private void OnPortNamesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.SystemPortsAvailable = this.barcodeReaderService.PortNames.Any();
        }

        private async Task SaveAsync()
        {
            try
            {
                this.Logger.Debug("Saving barcode reader settings ...");

                this.IsWaitingForResponse = true;
                await this.baysWebService.UpdateBarcodeReaderSettingsAsync(this.IsAccessoryEnabled, this.PortName);

                this.ShowNotification(VW.App.Resources.InstallationApp.SaveSuccessful);
                this.Logger.Debug("Barcode reader settings saved.");
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
