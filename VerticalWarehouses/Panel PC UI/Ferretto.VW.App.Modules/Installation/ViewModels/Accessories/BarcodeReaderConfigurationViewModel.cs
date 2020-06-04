using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class BarcodeReaderConfigurationViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBarcodeReaderService barcodeReaderService;

        private bool systemPortsAvailable;

        #endregion

        #region Constructors

        public BarcodeReaderConfigurationViewModel(
            IBarcodeReaderService barcodeReaderService)
            : base(PresentationMode.Installer)
        {
            this.barcodeReaderService = barcodeReaderService;
            this.barcodeReaderService.PortNames.CollectionChanged += this.OnPortNamesChanged;
        }

        #endregion

        #region Properties

        public IEnumerable<string> PortNames => this.barcodeReaderService.PortNames;

        public IEnumerable<object> BarcodeSteps { get; } = new List<object>()
        {
            new
            {
                Barcode = "pack://application:,,,/Ferretto.VW.App.Accessories;component/Barcode/Resources/newland_enter_setup.png",
                Title = VW.App.Resources.InstallationApp.AccessoriesBarcodeEnterSetup,
                Number = 1,
            },
            new
            {
                Barcode = "pack://application:,,,/Ferretto.VW.App.Accessories;component/Barcode/Resources/newland_usb_com_emulation.png",
                Title = VW.App.Resources.InstallationApp.AccessoriesBarcodeEnableComEmulation,
                Number = 2,
            },
            new
            {
                Barcode = "pack://application:,,,/Ferretto.VW.App.Accessories;component/Barcode/Resources/newland_exit_setup.png",
                Title = VW.App.Resources.InstallationApp.AccessoriesBarcodeExitSetup,
                Number = 3,
            },
        };

        public bool SystemPortsAvailable
        {
            get => this.systemPortsAvailable;
            set => this.SetProperty(ref this.systemPortsAvailable, value);
        }

        #endregion

        #region Methods

        private void OnPortNamesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.SystemPortsAvailable = this.barcodeReaderService.PortNames.Any();
        }

        #endregion
    }
}
