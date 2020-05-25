using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.App.Accessories.Barcode;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.Devices;
using Ferretto.VW.Devices.BarcodeReader;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Microsoft.AppCenter.Analytics;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Accessories
{
    internal sealed partial class BarcodeReaderService : IBarcodeReaderService
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly IMachineBarcodesWebService barcodesWebService;

        private readonly IBayManager bayManager;

        private readonly IBarcodeReaderDriver deviceDriver;

        private readonly IEventAggregator eventAggregator;

        private readonly ILoadingUnitBarcodeService loadingUnitBarcodeService;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly INavigationService navigationService;

        private readonly IPutToLightBarcodeService putToLightBarcodeService;

        private readonly object syncRoot = new object();

        private BarcodeRule activeRule;

        private bool isStarted;

        private IEnumerable<BarcodeRule> ruleSet = Array.Empty<BarcodeRule>();

        #endregion

        #region Constructors

        public BarcodeReaderService(
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            IMachineBaysWebService machineBaysWebService,
            IBarcodeReaderDriver deviceDriver,
            INavigationService navigationService,
            IPutToLightBarcodeService putToLightBarcodeService,
            ILoadingUnitBarcodeService loadingUnitBarcodeService,
            IAuthenticationService authenticationService,
            IMachineBarcodesWebService barcodesWebService)
        {
            this.eventAggregator = eventAggregator;
            this.bayManager = bayManager;
            this.machineBaysWebService = machineBaysWebService;
            this.deviceDriver = deviceDriver;
            this.navigationService = navigationService;
            this.putToLightBarcodeService = putToLightBarcodeService;
            this.loadingUnitBarcodeService = loadingUnitBarcodeService;
            this.authenticationService = authenticationService;
            this.barcodesWebService = barcodesWebService;

            this.deviceDriver.BarcodeReceived += async (sender, e) => await this.OnBarcodeReceivedAsync(e);
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

                if (this.deviceDriver is IQueryableDevice queryableDevice)
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

        public async Task StartAsync()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(BarcodeReaderService));
            }

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
                    this.deviceDriver.Connect(
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

        public Task StopAsync()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(BarcodeReaderService));
            }

            try
            {
                this.deviceDriver.Disconnect();
                this.ruleSet = Array.Empty<BarcodeRule>();

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

            return Task.CompletedTask;
        }

        private async Task ExecuteActionOnActiveContext(UserActionEventArgs eventArgs, IOperationalContextViewModel activeContext)
        {
            var handled = false;
            if (activeContext != null)
            {
                handled = true;
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    try
                    {
                        await activeContext.CommandUserActionAsync(eventArgs);

                        if (eventArgs.HasMismatch && eventArgs.RestartOnMismatch)
                        {
                            this.activeRule = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.Error(
                            ex,
                            $"Barcode {eventArgs.Code} caused an exception on context '{activeContext.GetType()}'");
                    }
                });
            }

            if (this.authenticationService.UserName != null)
            {
                handled = await this.putToLightBarcodeService.ProcessUserActionAsync(eventArgs);

                handled = handled || await this.loadingUnitBarcodeService.ProcessUserActionAsync(eventArgs);
            }

            if (!handled)
            {
                this.logger.Warn("The rule was not handled."); // TODO localize
            }
        }

        /// <summary>
        /// Gets the currently active view model.
        /// </summary>
        /// <returns>The reference to the currently active operational context of the view model.</returns>
        private INavigableViewModel GetActiveContext()
        {
            INavigableViewModel activeViewModel = null;
            Application.Current.Dispatcher.Invoke(() =>
            {
                activeViewModel = this.navigationService.GetActiveViewModel();
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

            if (matchedRule is null)
            {
                this.logger.Warn($"Barcode {barcode}: no matching rule found.");
            }
            else
            {
                this.logger.Debug($"Barcode {barcode}: matched rule action '{matchedRule.Action}' (context '{matchedRule.ContextName ?? "<global>"}').");
            }

            return matchedRule;
        }

        private async Task LoadRuleSetAsync()
        {
            try
            {
                var rules = await this.barcodesWebService.GetAllAsync();

                this.ruleSet = rules.OrderBy(r => r.Priority).ToArray();
            }
            catch (Exception ex)
            {
                this.logger.Error($"Unable to load barcode rules.");
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

        private async Task OnBarcodeReceivedAsync(ActionEventArgs e)
        {
            var code = e.Code
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty);

            var activeViewModel = this.GetActiveContext();
            var activeContext = activeViewModel as IOperationalContextViewModel;

            this.logger.Debug(
                $"Barcode '{code}': active context is '{activeContext?.GetType().Name ?? "<global>"}'.");

            UserActionEventArgs eventArgs;
            lock (this.syncRoot)
            {
                var chainedRuleIsExpected = this.activeRule?.NextRuleId != null;
                this.activeRule = this.SelectActiveRule(code, activeContext);

                if (this.activeRule is null)
                {
                    this.NotifyWarning(
                        string.Format(Resources.Localized.Get("OperatorApp.BarcodeNotRecognized"), code));

                    this.logger.Warn($"Barcode {e.Code}: does not match any rule.");

                    // send an event to notify the reset
                    eventArgs = new UserActionEventArgs(e.Code, isReset: chainedRuleIsExpected);
                }
                else
                {
                    this.logger.Debug(
                        $"Barcode '{e.Code}': matched action '{this.activeRule.Action}' (context '{this.activeRule.ContextName ?? "<global>"}')");

                    eventArgs = new UserActionEventArgs(e.Code, this.activeRule);
                }
            }

            Analytics.TrackEvent("Read Barcode", new Dictionary<string, string>
            {
                    { "Active Page", activeViewModel?.GetType()?.Name?.Replace("ViewModel", "View") ?? "<none>" },
                    { "Matched Rule", this.activeRule?.Action?.ToString() ?? "<none>" },
                    { "Machine Serial Number", this.bayManager.Identity?.SerialNumber }
            });

            await this.ExecuteActionOnActiveContext(eventArgs, activeContext);
        }

        private BarcodeRule SelectActiveRule(string code, IOperationalContextViewModel activeContext)
        {
            this.logger.Debug($"Barcode '{code}': active rule is '{this.activeRule?.Action ?? "<none>"}'.");

            if (this.activeRule is null)
            {
                this.logger.Debug($"Barcode '{code}': no active chain, starting a new one.");
                return this.GetActiveContextRule(code, activeContext?.ActiveContextName);
            }

            if (this.activeRule.NextRuleId is null)
            {
                this.logger.Debug($"Barcode '{code}': chain completed, starting a new one.");
                return this.GetActiveContextRule(code, activeContext?.ActiveContextName);
            }

            var nextRule = this.ruleSet.SingleOrDefault(r => r.Id == this.activeRule.NextRuleId);

            if (nextRule is null)
            {
                this.logger.Warn($"Barcode '{code}': next chained rule not found. Check your rule set.");

                return null;
            }

            this.logger.Debug($"Barcode '{code}': next chained rule action is '{nextRule.Action}'.");
            if (Regex.IsMatch(code, nextRule.Pattern))
            {
                this.logger.Debug($"Barcode '{code}': matched next chained rule.");
                return nextRule;
            }
            else
            {
                this.logger.Warn($"Barcode '{code}': did not match the chained rule.");

                if (this.activeRule.RestartOnMismatch)
                {
                    this.logger.Warn($"Barcode '{code}': resetting rule chain.");

                    return this.GetActiveContextRule(code, activeContext?.ActiveContextName);
                }
            }

            return this.activeRule;
        }

        #endregion
    }
}
