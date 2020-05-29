using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Menu.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class AccessoriesMenuViewModel : BaseInstallationMenuViewModel
    {
        #region Fields

        private readonly IMachineAccessoriesWebService accessoriesWebService;

        private BayAccessories accessories;

        private bool hasAccessories;

        private bool isAlphaNumericBarAvailable;

        private bool isBarcodeReaderAvailable;

        private bool isCardReaderAvailable;

        private bool isLabelPrinterAvailable;

        private bool isLaserPointerAvailable;

        private bool isTokenReaderAvailable;

        private bool isWeightingScaleAvailable;

        private DelegateCommand<string> openSettingsCommand;

        #endregion

        #region Constructors

        public AccessoriesMenuViewModel(IMachineAccessoriesWebService accessoriesWebService)
        {
            this.accessoriesWebService = accessoriesWebService;
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public bool HasAccessories
        {
            get => this.hasAccessories;
            private set => this.SetProperty(ref this.hasAccessories, value);
        }

        public bool IsAlphaNumericBarAvailable
        {
            get => this.isAlphaNumericBarAvailable;
            private set => this.SetProperty(ref this.isAlphaNumericBarAvailable, value, this.RefreshHasAccessories);
        }

        public bool IsBarcodeReaderAvailable
        {
            get => this.isBarcodeReaderAvailable;
            private set => this.SetProperty(ref this.isBarcodeReaderAvailable, value, this.RefreshHasAccessories);
        }

        public bool IsCardReaderAvailable
        {
            get => this.isCardReaderAvailable;
            private set => this.SetProperty(ref this.isCardReaderAvailable, value, this.RefreshHasAccessories);
        }

        public bool IsLabelPrinterAvailable
        {
            get => this.isLabelPrinterAvailable;
            private set => this.SetProperty(ref this.isLabelPrinterAvailable, value, this.RefreshHasAccessories);
        }

        public bool IsLaserPointerAvailable
        {
            get => this.isLaserPointerAvailable;
            private set => this.SetProperty(ref this.isLaserPointerAvailable, value, this.RefreshHasAccessories);
        }

        public bool IsTokenReaderAvailable
        {
            get => this.isTokenReaderAvailable;
            private set => this.SetProperty(ref this.isTokenReaderAvailable, value, this.RefreshHasAccessories);
        }

        public bool IsWeightingScaleAvailable
        {
            get => this.isWeightingScaleAvailable;
            private set => this.SetProperty(ref this.isWeightingScaleAvailable, value, this.RefreshHasAccessories);
        }

        public ICommand OpenSettingsCommand =>
            this.openSettingsCommand
            ??
            (this.openSettingsCommand = new DelegateCommand<string>(
                this.OpenSettings,
                this.CanOpenSettings));

        #endregion

        #region Methods

        public async override Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            try
            {
                this.accessories = await this.accessoriesWebService.GetAllAsync();

                this.IsAlphaNumericBarAvailable = this.accessories.AlphaNumericBar != null;
                this.IsBarcodeReaderAvailable = this.accessories.BarcodeReader != null;
                this.IsCardReaderAvailable = this.accessories.CardReader != null;
                this.IsLabelPrinterAvailable = this.accessories.LabelPrinter != null;
                this.IsLaserPointerAvailable = this.accessories.LaserPointer != null;
                this.IsTokenReaderAvailable = this.accessories.TokenReader != null;
                this.IsWeightingScaleAvailable = this.accessories.WeightingScale != null;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.openSettingsCommand?.RaiseCanExecuteChanged();
        }

        private bool CanOpenSettings(string viewModel)
        {
            return this.CanExecuteCommand();
        }

        private void OpenSettings(string viewModel)
        {
            this.NavigationService.Appear(nameof(Utils.Modules.Installation), viewModel, this.accessories);
        }

        private void RefreshHasAccessories()
        {
            this.HasAccessories =
                this.isAlphaNumericBarAvailable
                ||
                this.isBarcodeReaderAvailable
                ||
                this.isCardReaderAvailable
                ||
                this.isLabelPrinterAvailable
                ||
                this.isLaserPointerAvailable
                ||
                this.isTokenReaderAvailable
                ||
                this.isWeightingScaleAvailable;
        }

        #endregion
    }
}
