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

        private readonly IMachineBaysWebService machineBaysWebService;

        private BayAccessories accessories;

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

        public AccessoriesMenuViewModel(IMachineBaysWebService machineBaysWebService)
        {
            this.machineBaysWebService = machineBaysWebService;
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsAlphaNumericBarAvailable
        {
            get => this.isAlphaNumericBarAvailable;
            private set => this.SetProperty(ref this.isAlphaNumericBarAvailable, value);
        }

        public bool IsBarcodeReaderAvailable
        {
            get => this.isBarcodeReaderAvailable;
            private set => this.SetProperty(ref this.isBarcodeReaderAvailable, value);
        }

        public bool IsCardReaderAvailable
        {
            get => this.isCardReaderAvailable;
            private set => this.SetProperty(ref this.isCardReaderAvailable, value);
        }

        public bool IsLabelPrinterAvailable
        {
            get => this.isLabelPrinterAvailable;
            private set => this.SetProperty(ref this.isLabelPrinterAvailable, value);
        }

        public bool IsLaserPointerAvailable
        {
            get => this.isLaserPointerAvailable;
            private set => this.SetProperty(ref this.isLaserPointerAvailable, value);
        }

        public bool IsTokenReaderAvailable
        {
            get => this.isTokenReaderAvailable;
            private set => this.SetProperty(ref this.isTokenReaderAvailable, value);
        }

        public bool IsWeightingScaleAvailable
        {
            get => this.isWeightingScaleAvailable;
            private set => this.SetProperty(ref this.isWeightingScaleAvailable, value);
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
                this.accessories = await this.machineBaysWebService.GetAccessoriesAsync();

                this.IsAlphaNumericBarAvailable = this.accessories.AlphaNumericBar?.IsConfiguredNew ?? false;
                this.IsBarcodeReaderAvailable = this.accessories.BarcodeReader?.IsConfiguredNew ?? false;
                this.IsCardReaderAvailable = this.accessories.CardReader?.IsConfiguredNew ?? false;
                this.IsLabelPrinterAvailable = this.accessories.LabelPrinter?.IsConfiguredNew ?? false;
                this.IsLaserPointerAvailable = this.accessories.LaserPointer?.IsConfiguredNew ?? false;
                this.IsTokenReaderAvailable = this.accessories.TokenReader?.IsConfiguredNew ?? false;
                this.IsWeightingScaleAvailable = this.accessories.WeightingScale?.IsConfiguredNew ?? false;
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

        #endregion
    }
}
