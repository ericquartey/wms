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

        private IOperationalContextViewModel GetActiveContext()
        {
            IOperationalContextViewModel activeViewModel = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                activeViewModel = this.navigationService.GetActiveViewModel() as IOperationalContextViewModel;
            });

            return activeViewModel;
        }

        private BarcodeRule GetActiveContextRule(string barcode, string activeContextName)
        {
            System.Diagnostics.Debug.Assert(this.ruleSet != null);

            var validRuleSet = new List<BarcodeRule>();
            foreach (var rule in this.ruleSet)
            {
                if (Enum.TryParse<ContextAction>(rule.ContextName, out var contextAction))
                {
                    validRuleSet.Add(rule);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No valid context found for {rule.ContextName}");
                }
            }

            return validRuleSet.FirstOrDefault(r =>
                 r.ContextName == activeContextName
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

        private async Task OnBarcodeReceivedAsync(object sender, ActionEventArgs e)
        {
            var activeContext = this.GetActiveContext();

            if (activeContext is null)
            {
                System.Diagnostics.Debug.WriteLine($"Current view model does not specify an operational context.");
                return;
            }

            await this.LoadRuleSetAsync();

            var rule = this.GetActiveContextRule(e.Code, activeContext.ActiveContextName);

            if (rule is null)
            {
                System.Diagnostics.Debug.WriteLine($"Barcode {e.Code} does not match any rule.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Barcode {e.Code} matched rule: '{rule.ContextName}'");

                var match = Regex.Match(e.Code, rule.Pattern);
                System.Diagnostics.Debug.Assert(match.Success);

                var eventArgs = new UserActionEventArgs(e.Code, rule.Action);
                for (var i = 0; i < match.Groups.Count; i++)
                {
                    var group = match.Groups[i];
                    eventArgs.Parameters.Add(group.Name, group.Value);
                }

                await activeContext.CommandUserActionAsync(eventArgs);
            }
        }

        #endregion
    }
}
