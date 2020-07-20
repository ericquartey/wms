using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class LabelPrinterSettingsViewModel : AccessorySettingsViewModel
    {
        #region Fields

        private readonly IMachineAccessoriesWebService machineAccessoriesWebService;

        private readonly DelegateCommand printTestPageCommand;

        private string printerName;

        #endregion

        #region Constructors

        public LabelPrinterSettingsViewModel(
            IMachineAccessoriesWebService machineAccessoriesWebService)
        {
            this.machineAccessoriesWebService = machineAccessoriesWebService;

            this.printTestPageCommand = new DelegateCommand(async () => await this.PrintTestPageAsync(), this.CanPrintTestPage);
        }

        #endregion

        #region Properties

        public string PrinterName
        {
            get => this.printerName;
            set
            {
                if (this.SetProperty(ref this.printerName, value))
                {
                    this.AreSettingsChanged = true;
                }
            }
        }

        public System.Windows.Input.ICommand PrintTestPageCommand => this.printTestPageCommand;

        #endregion

        #region Methods

        protected override async Task OnDataRefreshAsync()
        {
            await base.OnDataRefreshAsync();

            try
            {
                this.IsEnabled = this.CanEnable();
                this.RaisePropertyChanged(nameof(this.IsEnabled));

                if (this.Data is BayAccessories bayAccessories)
                {
                    this.IsAccessoryEnabled = bayAccessories.LabelPrinter.IsEnabledNew;
                    this.printerName = bayAccessories.LabelPrinter.Name;
                    this.RaisePropertyChanged(nameof(this.PrinterName));
                }
                else
                {
                    this.Logger.Warn(VW.App.Resources.Localized.Get("Improper parameters were passed to the Label printer settings page. Leaving the page ..."));

                    this.NavigationService.GoBack();
                }

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
                this.Logger.Debug("Saving Label printer settings ...");

                this.IsWaitingForResponse = true;

                await this.machineAccessoriesWebService.UpdateLabelPrinterSettingsAsync(this.IsAccessoryEnabled, this.PrinterName);

                this.ShowNotification(VW.App.Resources.InstallationApp.SaveSuccessful);

                this.Logger.Debug("Label printer settings saved.");
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

        private bool CanPrintTestPage() => string.IsNullOrWhiteSpace(this.PrinterName);

        private async Task PrintTestPageAsync()
        {
            this.Logger.Debug("Printing test page ...");

            try
            {
                await this.machineAccessoriesWebService.PrintTestPageAsync(this.PrinterName);
                this.ShowNotification(VW.App.Resources.InstallationApp.SuccessfullChange);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        #endregion
    }
}
