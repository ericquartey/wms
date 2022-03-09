using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NetBarcode;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.Devices.BarcodeReader;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class BarcodeReaderSettingsViewModel : AccessorySettingsViewModel
    {
        #region Fields

        private readonly IBarcodeReaderService barcodeReaderService;

        private readonly IEventAggregator eventAggregator;

        private readonly ISerialPortsService serialPortsService;

        private SubscriptionToken barcodeSubscriptionToken;

        private DelegateCommand configureDeviceCommand;

        private DeviceModel deviceModel;

        private bool imageExist;

        private ImageSource imageSource;

        private string portName;

        private string receivedBarcode;

        private bool systemPortsAvailable;

        private DelegateCommand testBarcodeCommand;

        private string tryBarcodeImage;

        #endregion

        #region Constructors

        public BarcodeReaderSettingsViewModel(
            IBarcodeReaderService barcodeReaderService,
            ISerialPortsService serialPortsService,
            IEventAggregator eventAggregator)
        {
            this.barcodeReaderService = barcodeReaderService;
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

        public DeviceModel DeviceModel
        {
            get => this.deviceModel;
            set => this.SetProperty(ref this.deviceModel, value);
        }

        public bool ImageExist
        {
            get => this.imageExist;
            set => this.SetProperty(ref this.imageExist, value);
        }

        public ImageSource ImageSource
        {
            get => this.imageSource;
            set => this.SetProperty(ref this.imageSource, value);
        }

        public string PortName
        {
            get => this.portName;
            set => this.SetProperty(ref this.portName, value, () => this.AreSettingsChanged = true);
        }

        public IEnumerable<string> PortNames => this.serialPortsService.PortNames;

        public string ReceivedBarcode
        {
            get => this.receivedBarcode;
            set => this.SetProperty(ref this.receivedBarcode, value);
        }

        public bool SystemPortsAvailable
        {
            get => this.systemPortsAvailable;
            set => this.SetProperty(ref this.systemPortsAvailable, value);
        }

        public ICommand TestBarcodeCommand =>
                                                    this.testBarcodeCommand
            ??
            (this.testBarcodeCommand = new DelegateCommand(this.TestBarcode));

        public string TryBarcodeImage
        {
            get => this.tryBarcodeImage;
            set => this.SetProperty(ref this.tryBarcodeImage, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.ReceivedBarcode = null;

            this.barcodeSubscriptionToken?.Dispose();
            this.barcodeSubscriptionToken = null;

            this.TryBarcodeImage = string.Empty;
            this.ImageSource = new BitmapImage();
            this.ImageExist = false;
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.ReceivedBarcode = null;

            this.barcodeSubscriptionToken =
                this.barcodeSubscriptionToken
                ??
                this.eventAggregator
                    .GetEvent<PubSubEvent<ActionEventArgs>>()
                    .Subscribe(
                        this.OnBarcodeReceived,
                        ThreadOption.UIThread,
                        false);
        }

        public void TestBarcode()
        {
            try
            {
                if (!string.IsNullOrEmpty(this.TryBarcodeImage))
                {
                    var barcode = new Barcode(this.TryBarcodeImage, NetBarcode.Type.Code128, true);
                    var image = barcode.GetImage();

                    var stream = new MemoryStream();
                    image.Save(stream, ImageFormat.Jpeg);
                    stream.Position = 0;
                    if (stream != null)
                    {
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = stream;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();

                        this.ImageSource = bitmapImage;
                        this.ImageExist = true;
                    }
                }
                else
                {
                    this.ImageSource = new BitmapImage();
                    this.ImageExist = false;
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error("BarcodeImage Error: " + ex);
                this.ImageSource = new BitmapImage();
                this.ImageExist = false;
            }
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
                    bayAccessories.BarcodeReader != null)
                {
                    this.IsAccessoryEnabled = bayAccessories.BarcodeReader.IsEnabledNew;
                    this.PortName = bayAccessories.BarcodeReader.PortName;
                    this.DeviceModel = bayAccessories.BarcodeReader.DeviceInformation?.ModelNumber?.Contains("1550") == true
                        ? Devices.BarcodeReader.DeviceModel.Newland1550
                        : Devices.BarcodeReader.DeviceModel.NotSpecified;

                    this.SetDeviceInformation(bayAccessories.BarcodeReader.DeviceInformation);
                }
                else
                {
                    this.Logger.Warn("Improper parameters were passed to the barcode reader settings page.");
                }

                this.barcodeReaderService.DeviceModel = this.ModelNumber?.Contains("1550") == true
                  ? Devices.BarcodeReader.DeviceModel.Newland1550
                  : Devices.BarcodeReader.DeviceModel.NotSpecified;

                await this.barcodeReaderService.StartAsync();

                var liveInformation = this.barcodeReaderService.DeviceInformation;
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
                this.Logger.Debug("Saving barcode reader settings ...");
                this.ClearNotifications();
                this.IsWaitingForResponse = true;
                await this.barcodeReaderService.UpdateSettingsAsync(this.IsAccessoryEnabled, this.PortName, this.DeviceModel);

                this.Logger.Debug("Barcode reader settings saved.");

                var liveInformation = this.barcodeReaderService.DeviceInformation;
                this.FirmwareVersion = liveInformation.FirmwareVersion;
                this.SerialNumber = liveInformation.SerialNumber;
                this.ManufactureDate = liveInformation.ManufactureDate;
                this.ModelNumber = liveInformation.ModelNumber;

                this.DeviceModel = this.ModelNumber?.Contains("1550") == true
                    ? Devices.BarcodeReader.DeviceModel.Newland1550
                    : Devices.BarcodeReader.DeviceModel.NotSpecified;
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

        private void OnBarcodeReceived(ActionEventArgs e)
        {
            this.ReceivedBarcode = e.Code.Replace("\r", "<CR>").Replace("\n", "<LF>");
        }

        private void OnPortNamesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.SystemPortsAvailable = this.serialPortsService.PortNames.Any();
        }

        #endregion
    }
}
