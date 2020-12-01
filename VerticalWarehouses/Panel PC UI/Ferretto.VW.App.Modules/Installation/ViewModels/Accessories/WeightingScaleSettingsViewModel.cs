using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class WeightingScaleSettingsViewModel : AccessorySettingsViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly ISerialPortsService serialPortsService;

        private readonly IWeightingScaleService weightingScaleService;

        private SubscriptionToken barcodeSubscriptionToken;

        private DelegateCommand configureDeviceCommand;

        private string portName;

        private bool systemPortsAvailable;

        #endregion

        #region Constructors

        public WeightingScaleSettingsViewModel(
            IWeightingScaleService weightingScaleService,
            ISerialPortsService serialPortsService,
            IEventAggregator eventAggregator)
        {
            this.weightingScaleService = weightingScaleService;
            this.eventAggregator = eventAggregator;
            this.serialPortsService = serialPortsService;

            this.serialPortsService.PortNames.CollectionChanged += this.OnPortNamesChanged;
            this.SystemPortsAvailable = this.serialPortsService.PortNames.Any();
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
            set => this.SetProperty(ref this.portName, value, () => this.AreSettingsChanged = true);
        }

        public IEnumerable<string> PortNames => this.serialPortsService.PortNames;

        public bool SystemPortsAvailable
        {
            get => this.systemPortsAvailable;
            set => this.SetProperty(ref this.systemPortsAvailable, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.barcodeSubscriptionToken?.Dispose();
            this.barcodeSubscriptionToken = null;
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            //this.barcodeSubscriptionToken =
            //    this.barcodeSubscriptionToken
            //    ??
            //    this.eventAggregator
            //        .GetEvent<PubSubEvent<ActionEventArgs>>()
            //        .Subscribe(
            //            this.OnBarcodeReceived,
            //            ThreadOption.UIThread,
            //            false);
        }

        protected override async Task OnDataRefreshAsync()
        {
            await base.OnDataRefreshAsync();

            try
            {
                this.IsEnabled = this.CanEnable();
                this.RaisePropertyChanged(nameof(this.IsEnabled));

                if (this.Data is BayAccessories bayAccessories
                    &&
                    bayAccessories.WeightingScale != null)
                {
                    this.IsAccessoryEnabled = bayAccessories.WeightingScale.IsEnabledNew;
                    this.PortName = bayAccessories.WeightingScale.PortName;

                    this.SetDeviceInformation(bayAccessories.WeightingScale.DeviceInformation);
                }
                else
                {
                    this.Logger.Warn("Improper parameters were passed to Weighting Scale settings page.");
                }

                var liveInformation = this.weightingScaleService.DeviceInformation;
                this.FirmwareVersion = liveInformation.FirmwareVersion;
                this.SerialNumber = liveInformation.SerialNumber;
                this.ManufactureDate = liveInformation.ManufactureDate;
                this.ModelNumber = liveInformation.ModelNumber;

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
                this.Logger.Debug("Saving Weighting Scale settings ...");
                this.ClearNotifications();
                this.IsWaitingForResponse = true;
                await this.weightingScaleService.UpdateSettingsAsync(this.IsAccessoryEnabled, this.PortName);

                this.Logger.Debug("Weighting Scale settings saved.");

                var liveInformation = this.weightingScaleService.DeviceInformation;
                this.FirmwareVersion = liveInformation.FirmwareVersion;
                this.SerialNumber = liveInformation.SerialNumber;
                this.ManufactureDate = liveInformation.ManufactureDate;
                this.ModelNumber = liveInformation.ModelNumber;
            }
            catch
            {
                throw;
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
            this.SystemPortsAvailable = this.serialPortsService.PortNames.Any();
        }

        #endregion
    }
}
