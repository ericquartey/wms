using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.Devices;
using Ferretto.VW.Devices.BarcodeReader;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Accessories
{
    internal sealed partial class BarcodeReaderService : IBarcodeReaderService
    {
        #region Fields

        private readonly IMachineBarcodesWebService barcodesWebService;

        private readonly IEventAggregator eventAggregator;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly INavigationService navigationService;

        private readonly IBarcodeReaderDriver reader;

        private BarcodeRule activeRule;

        private bool isStarted;

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

        #region Properties

        public Devices.DeviceInformation DeviceInformation
        {
            get
            {
                if (!this.isStarted)
                {
                    throw new InvalidOperationException("Cannot retrieve device information because the barcode is not connected.");//TODO localize
                }

                if (this.reader is IQueryableDevice queryableDevice)
                {
                    return queryableDevice.Information;
                }
                else
                {
                    throw new NotSupportedException("The device driver does not support querying information");
                }
            }
        }

        #endregion

        #region Methods

        public void Disable()
        {
            try
            {
                this.reader.Disconnect();

                this.DisableSerialPortsTimer();
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
            }
            finally
            {
                this.isStarted = false;
            }
        }

        public async Task StartAsync()
        {
            if (this.isStarted)
            {
                return;
            }

            try
            {
                this.InitializeSerialPortsTimer();

                var accessories = await this.machineBaysWebService.GetAccessoriesAsync();

                if (accessories.BarcodeReader?.IsEnabledNew == true)
                {
                    this.reader.Connect(
                        new ConfigurationOptions
                        {
                            PortName = accessories.BarcodeReader.PortName
                        });
                    this.isStarted = true;
                }

                await this.LoadRuleSetAsync();
            }
            catch (Exception ex)
            {
                this.isStarted = false;
                this.NotifyError(ex);
            }
        }

        private static async Task ExecuteActionOnActiveContext(UserActionEventArgs eventArgs, IOperationalContextViewModel activeContext)
        {
            if (activeContext is null)
            {
                return;
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

        /// <summary>
        /// Gets the currently active view model.
        /// </summary>
        /// <returns>The reference to the currently active operational context of the view model.</returns>
        private IOperationalContextViewModel GetActiveContext()
        {
            IOperationalContextViewModel activeViewModel = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                activeViewModel = this.navigationService.GetActiveViewModel() as IOperationalContextViewModel;
            });

            return activeViewModel;
        }

        /// <summary>
        /// Gets the best matching barcode rule for the given context.
        /// </summary>
        /// <param name="barcode">The barcode to match.</param>
        /// <param name="activeContextName">The name of the active context.</param>
        /// <returns>The barcode rule that best matches the specified barcode and context, or <c>null</c> if not match was found.</returns>
        private BarcodeRule GetActiveContextRule(string barcode, string activeContextName)
        {
            System.Diagnostics.Debug.Assert(this.ruleSet != null);

            // note: rules with a context have priority
            var matchedRule = this.ruleSet
                .FirstOrDefault(r =>
                    r.ContextName != null && r.ContextName.Equals(activeContextName, StringComparison.InvariantCultureIgnoreCase)
                    &&
                    Regex.IsMatch(barcode, r.Pattern));

            // note: rules without a context are applied globally
            matchedRule = matchedRule ?? this.ruleSet
               .FirstOrDefault(r =>
                   r.ContextName == null
                   &&
                   Regex.IsMatch(barcode, r.Pattern));

            System.Diagnostics.Debug.WriteLineIf(
                matchedRule is null,
                $"No valid context found for {activeContextName}");

            return matchedRule;
        }

        private async Task LoadRuleSetAsync()
        {
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

        private void NotifyWarning(string message)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(message, Services.Models.NotificationSeverity.Warning));
        }

        private async Task OnBarcodeReceivedAsync(object sender, ActionEventArgs e)
        {
            var activeContext = this.GetActiveContext();

            System.Diagnostics.Debug.WriteLineIf(
                activeContext is null,
                $"Current view model does not specify an operational context.");

            var code = e.Code
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);

            var chainedRuleIsExpected = this.activeRule?.NextRuleId != null;
            this.activeRule = this.SelectActiveRule(code, activeContext);
            if (this.activeRule is null)
            {
                this.NotifyWarning(
                    string.Format(Resources.Localized.Get("OperatorApp.BarcodeNotRecognized"), code));

                System.Diagnostics.Debug.WriteLine($"Barcode {e.Code} does not match any rule.");

                var eventArgs = new UserActionEventArgs(e.Code, isReset: chainedRuleIsExpected);
                await ExecuteActionOnActiveContext(eventArgs, activeContext);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Barcode {e.Code} matched rule context '{this.activeRule.ContextName}', action '{this.activeRule.Action}'");

                var eventArgs = new UserActionEventArgs(e.Code, this.activeRule);
                await ExecuteActionOnActiveContext(eventArgs, activeContext);
            }
        }

        private BarcodeRule SelectActiveRule(string code, IOperationalContextViewModel activeContext)
        {
            if (this.activeRule is null)
            {
                return this.GetActiveContextRule(code, activeContext?.ActiveContextName);
            }

            var nextRule = this.ruleSet.SingleOrDefault(r => r.Id == this.activeRule.NextRuleId);
            if (nextRule is null)
            {
                return this.GetActiveContextRule(code, activeContext?.ActiveContextName);
            }

            System.Diagnostics.Debug.WriteLine($"Barcode chained rule found.");

            if (Regex.IsMatch(code, nextRule.Pattern))
            {
                return nextRule;
            }

            if (this.activeRule.RestartOnMismatch)
            {
                return null;
            }

            return this.activeRule;
        }

        #endregion
    }
}
