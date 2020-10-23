using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
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

        private DelegateCommand barcode1Command;

        private bool barcode1Visibility;

        private DelegateCommand barcode2Command;

        private bool barcode2Visibility;

        private DelegateCommand barcode3Command;

        private bool barcode3Visibility;

        private DelegateCommand barcode4Command;

        private bool barcode4Visibility;

        private IEnumerable<object> barcodeSteps;

        private bool image1Visibility;

        private bool image2Visibility;

        private bool image3Visibility;

        private bool image4Visibility;

        private bool isDatalogicPBT9100;

        private bool isDatalogicPBT9501;

        private bool isNewland1550;

        private bool isNewland1580;

        private bool isNewland3280;

        private bool isNewland3290;

        private int portNumberIndex;

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

        public ICommand Barcode1Command =>
           this.barcode1Command
           ??
           (this.barcode1Command = new DelegateCommand(
               () => this.SetImageVisibility(1),
               this.CanExecute));

        public bool Barcode1Visibility
        {
            get => this.barcode1Visibility;
            set => this.SetProperty(ref this.barcode1Visibility, value);
        }

        public ICommand Barcode2Command =>
           this.barcode2Command
           ??
           (this.barcode2Command = new DelegateCommand(
               () => this.SetImageVisibility(2),
               this.CanExecute));

        public bool Barcode2Visibility
        {
            get => this.barcode2Visibility;
            set => this.SetProperty(ref this.barcode2Visibility, value);
        }

        public ICommand Barcode3Command =>
           this.barcode3Command
           ??
           (this.barcode3Command = new DelegateCommand(
               () => this.SetImageVisibility(3),
               this.CanExecute));

        public bool Barcode3Visibility
        {
            get => this.barcode3Visibility;
            set => this.SetProperty(ref this.barcode3Visibility, value);
        }

        public ICommand Barcode4Command =>
           this.barcode4Command
           ??
           (this.barcode4Command = new DelegateCommand(
               () => this.SetImageVisibility(4),
               this.CanExecute));

        public bool Barcode4Visibility
        {
            get => this.barcode4Visibility;
            set => this.SetProperty(ref this.barcode4Visibility, value);
        }

        public IEnumerable<object> BarcodeSteps
        {
            get => this.barcodeSteps;
            set => this.SetProperty(ref this.barcodeSteps, value);
        }

        public bool Image1Visibility
        {
            get => this.image1Visibility;
            set => this.SetProperty(ref this.image1Visibility, value);
        }

        public bool Image2Visibility
        {
            get => this.image2Visibility;
            set => this.SetProperty(ref this.image2Visibility, value);
        }

        public bool Image3Visibility
        {
            get => this.image3Visibility;
            set => this.SetProperty(ref this.image3Visibility, value);
        }

        public bool Image4Visibility
        {
            get => this.image4Visibility;
            set => this.SetProperty(ref this.image4Visibility, value);
        }

        public bool IsDatalogicPBT9100
        {
            get => this.isDatalogicPBT9100;
            set => this.SetProperty(ref this.isDatalogicPBT9100, value, this.UpdateBarcodes);
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

        public int PortNumberIndex
        {
            get => this.portNumberIndex;
            set => this.SetProperty(ref this.portNumberIndex, value);
        }

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

        private bool CanExecute()
        {
            return true;
        }

        private void ChangeBarcodeVisibility()
        {
            this.Image1Visibility = true;
            this.Image2Visibility = false;
            this.Image3Visibility = false;
            this.Image4Visibility = false;
        }

        private IEnumerable<object> GetBarcodeSteps(DeviceModel deviceModel)
        {
            if (deviceModel == DeviceModel.DatalogicPBT9501)
            {
                return new List<object>()
                {
                    new
                    {
                        Barcode = $"pack://application:,,,/Ferretto.VW.App.Accessories;Component/Barcode/Resources/{deviceModel}_usb_com_emulation.png",
                        Title = VW.App.Resources.InstallationApp.AccessoriesBarcodeEnableComEmulation,
                        Number = 2,
                        ImageHeight = 300,
                    },
                };
            }
            else if (deviceModel == DeviceModel.DatalogicPBT9100)
            {
                return new List<object>()
                {
                    new
                    {
                        Barcode = $"pack://application:,,,/Ferretto.VW.App.Accessories;Component/Barcode/Resources/{deviceModel}_usb_com_emulation.png",
                        Title = VW.App.Resources.InstallationApp.AccessoriesBarcodeEnableComEmulation,
                        Number = 2,
                        ImageHeight = 300,
                    },
                };
            }
            else if (deviceModel == DeviceModel.Newland1580 || deviceModel == DeviceModel.Newland3280)
            {
                return new List<object>()
                {
                    new
                    {
                        Barcode = $"pack://application:,,,/Ferretto.VW.App.Accessories;Component/Barcode/Resources/{deviceModel}_enter_setup.png",
                        Title = VW.App.Resources.InstallationApp.AccessoriesBarcodeEnterSetup,
                        Number = 2,
                        ImageHeight = 80,
                    },
                    new
                    {
                        Barcode = $"pack://application:,,,/Ferretto.VW.App.Accessories;Component/Barcode/Resources/{deviceModel}_energy_saving.png",
                        Title = VW.App.Resources.InstallationApp.AccessoriesBarcodeEnergySaving,
                        Number = 3,
                        ImageHeight = 80,
                    },
                    new
                    {
                        Barcode = $"pack://application:,,,/Ferretto.VW.App.Accessories;Component/Barcode/Resources/{deviceModel}_usb_com_emulation.png",
                        Title = VW.App.Resources.InstallationApp.AccessoriesBarcodeEnableComEmulation,
                        Number = 4,
                        ImageHeight = 80,
                    },
                    new
                    {
                        Barcode = $"pack://application:,,,/Ferretto.VW.App.Accessories;Component/Barcode/Resources/{deviceModel}_exit_setup.png",
                        Title = VW.App.Resources.InstallationApp.AccessoriesBarcodeExitSetup,
                        Number = 5,
                        ImageHeight = 80,
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
                        ImageHeight = 120,
                    },
                    new
                    {
                        Barcode = $"pack://application:,,,/Ferretto.VW.App.Accessories;Component/Barcode/Resources/{deviceModel}_usb_com_emulation.png",
                        Title = VW.App.Resources.InstallationApp.AccessoriesBarcodeEnableComEmulation,
                        Number = 3,
                        ImageHeight = 120,
                    },
                    new
                    {
                        Barcode = $"pack://application:,,,/Ferretto.VW.App.Accessories;Component/Barcode/Resources/{deviceModel}_exit_setup.png",
                        Title = VW.App.Resources.InstallationApp.AccessoriesBarcodeExitSetup,
                        Number = 4,
                        ImageHeight = 120,
                    },
                };
            }
        }

        private void OnPortNamesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.SystemPortsAvailable = this.serialPortsService.PortNames.Any();
        }

        private void SetImageVisibility(int id)
        {
            switch (id)
            {
                case 1:
                    if (this.Image1Visibility)
                    {
                        this.Image1Visibility = false;
                    }
                    else
                    {
                        this.Image1Visibility = true;
                    }

                    this.Image2Visibility = false;
                    this.Image3Visibility = false;
                    this.Image4Visibility = false;
                    break;

                case 2:
                    this.Image1Visibility = false;

                    if (this.Image2Visibility)
                    {
                        this.Image2Visibility = false;
                    }
                    else
                    {
                        this.Image2Visibility = true;
                    }

                    this.Image3Visibility = false;
                    this.Image4Visibility = false;
                    break;

                case 3:
                    this.Image1Visibility = false;
                    this.Image2Visibility = false;

                    if (this.Image3Visibility)
                    {
                        this.Image3Visibility = false;
                    }
                    else
                    {
                        this.Image3Visibility = true;
                    }

                    this.Image4Visibility = false;
                    break;

                case 4:
                    this.Image1Visibility = false;
                    this.Image2Visibility = false;
                    this.Image3Visibility = false;

                    if (this.Image4Visibility)
                    {
                        this.Image4Visibility = false;
                    }
                    else
                    {
                        this.Image4Visibility = true;
                    }

                    break;
            }
        }

        private void SetVisibility()
        {
            this.PortNumberIndex = this.barcodeSteps.Count() + 2;

            this.RaisePropertyChanged(nameof(this.PortNumberIndex));

            var count = this.barcodeSteps.Count();

            switch (count)
            {
                case 1:
                    this.Barcode1Visibility = true;
                    this.Barcode2Visibility = false;
                    this.Barcode3Visibility = false;
                    this.Barcode4Visibility = false;
                    break;

                case 2:
                    this.Barcode1Visibility = true;
                    this.Barcode2Visibility = true;
                    this.Barcode3Visibility = false;
                    this.Barcode4Visibility = false;
                    break;

                case 3:
                    this.Barcode1Visibility = true;
                    this.Barcode2Visibility = true;
                    this.Barcode3Visibility = true;
                    this.Barcode4Visibility = false;
                    break;

                case 4:
                    this.Barcode1Visibility = true;
                    this.Barcode2Visibility = true;
                    this.Barcode3Visibility = true;
                    this.Barcode4Visibility = true;
                    break;
            }
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
            else if (this.IsDatalogicPBT9100)
            {
                deviceModel = DeviceModel.DatalogicPBT9100;
            }

            this.BarcodeSteps = this.GetBarcodeSteps(deviceModel);

            this.ChangeBarcodeVisibility();

            this.SetVisibility();
        }

        #endregion
    }
}
