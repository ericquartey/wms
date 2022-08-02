using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class LabelPrinterSettingsViewModel : AccessorySettingsViewModel
    {
        #region Fields

        private readonly IBayManager bayManagerService;

        private readonly IMachineAccessoriesWebService machineAccessoriesWebService;

        private string printerName;

        #endregion

        #region Constructors

        public LabelPrinterSettingsViewModel(IBayManager bayManagerService,
               IMachineAccessoriesWebService machineAccessoriesWebService)
        {
            this.machineAccessoriesWebService = machineAccessoriesWebService;
            this.bayManagerService = bayManagerService;
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

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();
        }

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

        protected override async Task TestAsync()
        {
            try
            {
                this.Logger.Debug("Testing Label printer ...");

                this.IsWaitingForResponse = true;

                var bay = await this.bayManagerService.GetBayAsync();

                var bayNumber = bay.Number;

                await this.machineAccessoriesWebService.PrintTestPageAsync(bayNumber);

                this.ShowNotification(VW.App.Resources.InstallationApp.TestSuccessful);

                this.Logger.Debug("Label printer Test.");
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
