using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class BarcodeReaderSettingsViewModel : AccessorySettingsViewModel
    {
        #region Fields

        private readonly IBarcodeReaderService barcodeReaderService;

        private DelegateCommand configureDeviceCommand;

        private string portName;

        private string selectedPortName;

        private bool systemPortsAvailable;

        #endregion

        #region Constructors

        public BarcodeReaderSettingsViewModel(IBarcodeReaderService barcodeReaderService)
        {
            this.barcodeReaderService = barcodeReaderService;

            this.barcodeReaderService.PortNames.CollectionChanged += this.OnPortNamesChanged;
            this.SystemPortsAvailable = this.barcodeReaderService.PortNames.Any();
        }

        #endregion

        #region Properties

        public ICommand ConfigureDeviceCommand =>
            this.configureDeviceCommand
            ??
            (this.configureDeviceCommand = new DelegateCommand(
                this.ConfigureDevice,
                this.CanConfigureDevice));

        public string PortName
        {
            get => this.portName;
            set => this.SetProperty(ref this.portName, value);
        }

        public IEnumerable<string> PortNames => this.barcodeReaderService.PortNames;

        public string SelectedPortName
        {
            get => this.selectedPortName;
            set => this.SetProperty(ref this.selectedPortName, value);
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
                    this.IsAccessoryEnabled = bayAccessories.BarcodeReader.IsEnabledNew;
                    this.PortName = bayAccessories.BarcodeReader.PortName;

                    this.SetDeviceInformation(bayAccessories.BarcodeReader.DeviceInformation);
                }
                else
                {
                    this.Logger.Warn("Improper parameters were passed to the barcode reader settings page. Leaving the page ...");

                    this.NavigationService.GoBack();
                }

                await this.barcodeReaderService.StartAsync();

                this.AreSettingsChanged = false;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected override async Task SaveAsync()
        {
            try
            {
                this.Logger.Debug("Saving barcode reader settings ...");

                this.IsWaitingForResponse = true;
                await this.barcodeReaderService.UpdateSettingsAsync(this.IsAccessoryEnabled, this.PortName);

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

        private bool CanConfigureDevice()
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

        #endregion
    }
}
