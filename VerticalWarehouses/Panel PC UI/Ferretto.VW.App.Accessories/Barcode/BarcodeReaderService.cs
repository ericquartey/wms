using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.BarcodeReader;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Accessories
{
    internal class BarcodeReaderService : IBarcodeReaderService
    {
        #region Fields

        private readonly IMachineBarcodesWebService barcodesWebService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly INavigationService navigationService;

        private readonly IBarcodeReaderDriver reader;

        private IEnumerable<BarcodeRule> ruleSet;

        #endregion

        #region Constructors

        public BarcodeReaderService(
            IEventAggregator eventAggregator,
            IMachineBaysWebService machineBaysWebService,
            IBarcodeReaderDriver reader,
            INavigationService navigationService,
            IMachineBarcodesWebService barcodesWebService)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.barcodesWebService = barcodesWebService ?? throw new ArgumentNullException(nameof(barcodesWebService));

            this.reader.BarcodeReceived += async (sender, e) => await this.OnBarcodeReceivedAsync(sender, e);
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

        public async Task StartAsync()
        {
            try
            {
                var accessories = await this.machineBaysWebService.GetAccessoriesAsync();

                // if(accessories.BarcodeReader.IsEnabled) // TODO restore this
                {
                    this.reader.Connect(new ConfigurationOptions
                    {
                        PortName = accessories.BarcodeReader.PortName
                    });
                }
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

            var matchedRule = this.ruleSet.FirstOrDefault(r =>
                 r.ContextName == activeContextName
                 &&
                 Regex.IsMatch(barcode, r.Pattern));

            System.Diagnostics.Debug.WriteLineIf(
                matchedRule is null,
                $"No valid context found for {activeContextName}");

            return matchedRule;
        }

        private async Task LoadRuleSetAsync()
        {
            if (this.ruleSet != null)
            {
                return;
            }

            try
            {
                this.ruleSet = await this.barcodesWebService.GetAllAsync();
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
            var code = e.Code.Replace("\r", "").Replace("\n", "");

            var activeContext = this.GetActiveContext();

            if (activeContext is null)
            {
                this.eventAggregator
                   .GetEvent<PresentationNotificationPubSubEvent>()
                   .Publish(new PresentationNotificationMessage(string.Format(Resources.OperatorApp.CurrentPageDoesNotSupportBarcodeScanning, code), Services.Models.NotificationSeverity.Warning));

                System.Diagnostics.Debug.WriteLine($"Current view model does not specify an operational context.");
                return;
            }

            await this.LoadRuleSetAsync();

            var rule = this.GetActiveContextRule(e.Code, activeContext.ActiveContextName);

            if (rule is null)
            {
                this.eventAggregator
                   .GetEvent<PresentationNotificationPubSubEvent>()
                   .Publish(new PresentationNotificationMessage(string.Format(Resources.OperatorApp.BarcodeNotRecognized, code), Services.Models.NotificationSeverity.Warning));

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

                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    try
                    {
                        await activeContext.CommandUserActionAsync(eventArgs);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Barcode {eventArgs.Code} caused an exception on {activeContext.GetType()}: {ex.Message}");
                    }
                });
            }
        }

        #endregion
    }
}
