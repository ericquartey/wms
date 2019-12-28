using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.BarcodeReader;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Accessories
{
    internal class BarcodeReaderService : IBarcodeReaderService
    {
        #region Fields

        private readonly IBarcodesWmsWebService barcodesWmsWebService;

        private readonly IEventAggregator eventAggregator;

        private readonly INavigationService navigationService;

        private readonly IBarcodeConfigurationOptions options;

        private readonly IBarcodeReader reader;

        private IEnumerable<BarcodeRule> ruleSet;

        #endregion

        #region Constructors

        public BarcodeReaderService(
            IEventAggregator eventAggregator,
            IBarcodeReader reader,
            INavigationService navigationService,
            IBarcodesWmsWebService barcodesWmsWebService,
            IBarcodeConfigurationOptions options)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.barcodesWmsWebService = barcodesWmsWebService ?? throw new ArgumentNullException(nameof(barcodesWmsWebService));

            this.reader.BarcodeReceived += async (sender, e) => await this.OnBarcodeReceivedAsync(sender, e);

            this.Enable();
        }

        #endregion

        #region Methods

        public void Disable()
        {
            try
            {
                this.reader.Disconnect();
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
            }
        }

        public void Enable()
        {
            try
            {
                this.reader.Connect(this.options);
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
            }
        }

        private BarcodeRule GetActiveContextRule(string barcode)
        {
            IOperationalContextViewModel activeViewModel = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                activeViewModel = this.navigationService.GetActiveViewModel() as IOperationalContextViewModel;
            });

            if (activeViewModel is null)
            {
                System.Diagnostics.Debug.WriteLine($"Current view model does not specify an operational context.");
                return null;
            }

            System.Diagnostics.Debug.Assert(this.ruleSet != null);

            return this.ruleSet.FirstOrDefault(r =>
                r.ContextName == activeViewModel.ActiveContextName
                &&
                Regex.IsMatch(barcode, r.Pattern));
        }

        private async Task LoadRuleSetAsync()
        {
            if (this.ruleSet != null)
            {
                return;
            }

            try
            {
                this.ruleSet = await this.barcodesWmsWebService.GetAllAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unable to load barcode rules.");
                this.NotifyError(ex);
            }
        }

        private void NotifyError(Exception ex)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(ex));
        }

        private async Task OnBarcodeReceivedAsync(object sender, BarcodeEventArgs e)
        {
            await this.LoadRuleSetAsync();

            var rule = this.GetActiveContextRule(e.Barcode);
            if (rule is null)
            {
                System.Diagnostics.Debug.WriteLine($"Barcode {e.Barcode} does not match any rule.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Barcode {e.Barcode} matched rule: '{rule.ContextName}'");

                // 3. publish event
                var eventArgs = new BarcodeMatchEventArgs(e.Barcode, rule.Action);
                this.eventAggregator
                    .GetEvent<PubSubEvent<BarcodeMatchEventArgs>>()
                    .Publish(eventArgs);
            }
        }

        #endregion
    }
}
