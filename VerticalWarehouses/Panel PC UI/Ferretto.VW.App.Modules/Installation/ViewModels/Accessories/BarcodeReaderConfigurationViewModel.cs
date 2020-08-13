using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.BarcodeReader;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class BarcodeReaderConfigurationViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBarcodeReaderService barcodeReaderService;

        private readonly ISerialPortsService serialPortsService;

        private IEnumerable<object> barcodeSteps;

        private bool isDatalogicPBT9501;

        private bool isNewland1550;

        private bool isNewland1580;

        private bool isNewland3280;

        private bool isNewland3290;

        private bool systemPortsAvailable;

        #endregion

        #region Constructors

        public BarcodeReaderConfigurationViewModel(
            IBarcodeReaderService barcodeReaderService,
            ISerialPortsService serialPortsService)
            : base(PresentationMode.Installer)
        {
            this.barcodeReaderService = barcodeReaderService;
            this.serialPortsService = serialPortsService;
            this.serialPortsService.PortNames.CollectionChanged += this.OnPortNamesChanged;
        }

        #endregion

        #region Properties

        public IEnumerable<object> BarcodeSteps
        {
            get => this.barcodeSteps;
            set => this.SetProperty(ref this.barcodeSteps, value);
        }

        public bool IsDatalogicPBT9501
        {
            get => this.isDatalogicPBT9501;
            set => this.SetProperty(ref this.isDatalogicPBT9501, value, this.UpdateBarcodes);
        }

        public bool IsNewland1550
        {
            get => this.isNewland1550;
            set => this.SetProperty(ref this.isNewland1550, value, this.UpdateBarcodes);
        }

        public bool IsNewland1580
        {
            get => this.isNewland1580;
            set => this.SetProperty(ref this.isNewland1580, value, this.UpdateBarcodes);
        }

        public bool IsNewland3280
        {
            get => this.isNewland3280;
            set => this.SetProperty(ref this.isNewland3280, value, this.UpdateBarcodes);
        }

        public bool IsNewland3290
        {
            get => this.isNewland3290;
            set => this.SetProperty(ref this.isNewland3290, value, this.UpdateBarcodes);
        }

        public IEnumerable<string> PortNames => this.serialPortsService.PortNames;

        public bool SystemPortsAvailable
        {
            get => this.systemPortsAvailable;
            set => this.SetProperty(ref this.systemPortsAvailable, value);
        }

        #endregion

        #region Methods

        public override Task OnAppearedAsync()
        {
            this.IsNewland1550 = true;

            return base.OnAppearedAsync();
        }

        private IEnumerable<object> GetBarcodeSteps(DeviceModel deviceModel)
        {
            if (deviceModel == DeviceModel.DatalogicPBT9501)
            {
                return new List<object>()
                {
                    new
                    {
                        Barcode = $"pack://application:,,,/Ferretto.VW.App.Accessories;component/Barcode/Resources/{deviceModel}_usb_com_emulation.png",
                        Title = VW.App.Resources.InstallationApp.AccessoriesBarcodeEnableComEmulation,
                        Number = 2,
                    },
                };
            }
            else
            {
                return new List<object>()
            {
                new
                {
                    Barcode = $"pack://application:,,,/Ferretto.VW.App.Accessories;Component/Barcode/Resources/{deviceModel}_enter_setup.png",
                    Title = VW.App.Resources.InstallationApp.AccessoriesBarcodeEnterSetup,
                    Number = 2,
                },
                new
                {
                    Barcode = $"pack://application:,,,/Ferretto.VW.App.Accessories;Component/Barcode/Resources/{deviceModel}_usb_com_emulation.png",
                    Title = VW.App.Resources.InstallationApp.AccessoriesBarcodeEnableComEmulation,
                    Number = 3,
                },
                new
                {
                    Barcode = $"pack://application:,,,/Ferretto.VW.App.Accessories;Component/Barcode/Resources/{deviceModel}_exit_setup.png",
                    Title = VW.App.Resources.InstallationApp.AccessoriesBarcodeExitSetup,
                    Number = 4,
                },
            };
            }
        }

        private void OnPortNamesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.SystemPortsAvailable = this.serialPortsService.PortNames.Any();
        }

        private void UpdateBarcodes()
        {
            var deviceModel = DeviceModel.Newland1550;
            if (this.IsNewland1550)
            {
                deviceModel = DeviceModel.Newland1550;
            }
            else if (this.IsNewland1580)
            {
                deviceModel = DeviceModel.Newland1580;
            }
            else if (this.IsNewland3280)
            {
                deviceModel = DeviceModel.Newland3280;
            }
            else if (this.IsNewland3290)
            {
                deviceModel = DeviceModel.Newland3290;
            }
            else if (this.IsDatalogicPBT9501)
            {
                deviceModel = DeviceModel.DatalogicPBT9501;
            }

            this.BarcodeSteps = this.GetBarcodeSteps(deviceModel);
        }

        #endregion
    }
}
