using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using CommonServiceLocator;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.Devices;
using Ferretto.VW.Devices.BarcodeReader;
using Ferretto.VW.MAS.AutomationService.Contracts;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Accessories
{
    internal sealed partial class BarcodeReaderService : IBarcodeReaderService
    {
        #region Fields

        private readonly IMachineAccessoriesWebService accessoriesWebService;

        private readonly IAuthenticationService authenticationService;

        private readonly IMachineBarcodesWebService barcodesWebService;

        private readonly IBayManager bayManager;

        private readonly IBarcodeReaderDriver deviceDriver;

        private readonly IEventAggregator eventAggregator;

        private readonly IHealthProbeService healthProbeService;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly INavigationService navigationService;

        private readonly object syncRoot = new object();

        private readonly Timer wmsStatusTimer;

        private BarcodeRule activeRule;

        private bool isDeviceEnabled;

        private bool isStarted;

        private bool needLoadRules;

        private HealthStatus oldWmsStatus;

        private IEnumerable<BarcodeRule> ruleSet = Array.Empty<BarcodeRule>();

        #endregion

        #region Constructors

        public BarcodeReaderService(
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            IMachineAccessoriesWebService accessoriesWebService,
            IMachineBaysWebService machineBaysWebService,
            IBarcodeReaderDriver deviceDriver,
            INavigationService navigationService,
            IAuthenticationService authenticationService,
            IMachineBarcodesWebService barcodesWebService,
            IHealthProbeService healthProbeService)
        {
            this.eventAggregator = eventAggregator;
            this.bayManager = bayManager;
            this.accessoriesWebService = accessoriesWebService;
            this.machineBaysWebService = machineBaysWebService;
            this.deviceDriver = deviceDriver;
            this.navigationService = navigationService;
            this.authenticationService = authenticationService;
            this.barcodesWebService = barcodesWebService;
            this.healthProbeService = healthProbeService;

            this.deviceDriver.BarcodeReceived += async (sender, e) => await this.OnBarcodeReceivedAsync(e);
            this.oldWmsStatus = HealthStatus.Unknown;

            this.wmsStatusTimer = new Timer(4000);
            this.wmsStatusTimer.Elapsed += this.WmsStatusTimer_Elapsed;
        }

        #endregion

        #region Properties

        public Devices.DeviceInformation DeviceInformation
        {
            get
            {
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

        public DeviceModel DeviceModel { get; set; }

        #endregion

        #region Methods

        public void SimulateRead(string barcode)
        {
            this.deviceDriver.SimulateRead(barcode);
        }

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
                this.logger.Info("Start barcode service");
                var accessories = await this.accessoriesWebService.GetAllAsync();
                this.isDeviceEnabled = accessories.BarcodeReader?.IsEnabledNew == true;
                if (this.isDeviceEnabled)
                {
                    if (!this.wmsStatusTimer.Enabled)
                    {
                        this.wmsStatusTimer.Start();
                    }
                    await this.LoadRuleSetAsync();

                    this.deviceDriver.Connect(
                        new NewlandSerialPortOptions
                        {
                            PortName = accessories.BarcodeReader.PortName,
                            DeviceModel = this.DeviceModel
                        });
                    this.isStarted = true;

                    if (!this.DeviceInformation.IsEmpty)
                    {
                        await this.accessoriesWebService.UpdateBarcodeReaderDeviceInfoAsync(
                            new MAS.AutomationService.Contracts.DeviceInformation
                            {
                                FirmwareVersion = this.DeviceInformation.FirmwareVersion,
                                ManufactureDate = this.DeviceInformation.ManufactureDate,
                                ModelNumber = this.DeviceInformation.ModelNumber,
                                SerialNumber = this.DeviceInformation.SerialNumber
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                this.isStarted = false;
                this.NotifyError($"{Resources.Localized.Get("OperatorApp.BarcodeServiceError")}. {ex.Message}");
            }
        }

        public Task StopAsync()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(BarcodeReaderService));
            }

            if (!this.isStarted)
            {
                return Task.CompletedTask;
            }

            try
            {
                this.logger.Info("Stop barcode service");
                this.deviceDriver.Disconnect();
                this.ruleSet = Array.Empty<BarcodeRule>();

                if (this.wmsStatusTimer.Enabled)
                {
                    this.wmsStatusTimer.Stop();
                }
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

        public async Task UpdateSettingsAsync(bool isEnabled, string portName, DeviceModel model)
        {
            try
            {
                await this.accessoriesWebService.UpdateBarcodeReaderSettingsAsync(isEnabled, portName);

                this.DeviceModel = model;
                this.isDeviceEnabled = isEnabled;
                if (this.isDeviceEnabled)
                {
                    await this.StartAsync();
                }
                else
                {
                    await this.StopAsync();
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
            }
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
                var putToLightBarcodeService = ServiceLocator.Current.GetService(typeof(IPutToLightBarcodeService)) as IPutToLightBarcodeService;
                handled = await putToLightBarcodeService.ProcessUserActionAsync(eventArgs);

                var loadingUnitBarcodeService = ServiceLocator.Current.GetService(typeof(ILoadingUnitBarcodeService)) as ILoadingUnitBarcodeService;
                handled = handled || await loadingUnitBarcodeService.ProcessUserActionAsync(eventArgs);
            }

            if (!handled)
            {
                this.logger.Warn("The rule was not handled.");
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
            foreach (var rule in this.ruleSet
                .Where(r => r.ContextName != null && r.ContextName.Equals(activeContextName, StringComparison.InvariantCultureIgnoreCase)))
            {
                var pattern = new Regex(rule.Pattern);
                if (pattern.IsMatch(barcode))
                {
                    this.logger.Debug($"Barcode '{barcode}': matched rule action '{rule.Action}' (context '{rule.ContextName ?? "<global>"}').");
                    return rule;
                }
            }

            var matchedRule = this.ruleSet
                .FirstOrDefault(r => r.ContextName == null
                    &&
                    Regex.IsMatch(barcode, r.Pattern));
            if (matchedRule is null)
            {
                this.logger.Warn($"Barcode '{barcode}': no matching rule found for context {activeContextName}.");
            }
            else
            {
                this.logger.Debug($"Barcode '{barcode}': matched rule action '{matchedRule.Action}' (context '{matchedRule.ContextName ?? "<global>"}').");
            }
            return matchedRule;
        }

        private async Task LoadRuleSetAsync()
        {
            try
            {
                var rules = await this.barcodesWebService.GetAllAsync();

                this.ruleSet = rules.OrderBy(r => r.Priority).ToArray();

                if (this.ruleSet.Any())
                {
                    this.logger.Debug($"{this.ruleSet.Count()} Barcode rules available:");

                    foreach (var rule in this.ruleSet)
                    {
                        this.logger.Trace($"Id: {rule.Id}; Action: {rule.Action}; ContextName: {rule.ContextName}; Pattern: {rule.Pattern}");
                    }
                }
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

        private void NotifyError(string message)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(message, Services.Models.NotificationSeverity.Error));
        }

        private void NotifyWarning(string message)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(message, Services.Models.NotificationSeverity.Warning));
        }

        private async Task OnBarcodeReceivedAsync(ActionEventArgs e)
        {
            this.eventAggregator
                .GetEvent<PubSubEvent<ActionEventArgs>>()
                .Publish(e);

            var code = e.Code
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);

            // do not process rules if the barcode configuration screens are active
            var skipProcessing = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                skipProcessing =
                    this.navigationService.IsActiveView(nameof(Utils.Modules.Installation), Utils.Modules.Accessories.BarcodeReader)
                    ||
                    this.navigationService.IsActiveView(nameof(Utils.Modules.Installation), Utils.Modules.Accessories.BarcodeReaderConfig);
            });

            if (skipProcessing)
            {
                return;
            }

            if (!this.ruleSet.Any(r => r.Id != 0) ||
                this.needLoadRules)
            {
                await this.LoadRuleSetAsync();
                this.needLoadRules = false;
            }

            this.oldWmsStatus = this.healthProbeService.HealthWmsStatus;

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

                    this.logger.Warn($"Barcode {code}: does not match any rule.");

                    // send an event to notify the reset
                    eventArgs = new UserActionEventArgs(code, isReset: chainedRuleIsExpected);
                }
                else
                {
                    this.logger.Debug(
                        $"Barcode '{code}': matched action '{this.activeRule.Action}' (context '{this.activeRule.ContextName ?? "<global>"}')");

                    eventArgs = new UserActionEventArgs(code, this.activeRule);
                }
            }

            await this.ExecuteActionOnActiveContext(eventArgs, activeContext);
        }

        private BarcodeRule SelectActiveRule(string code, IOperationalContextViewModel activeContext)
        {
            this.logger.Debug($"Barcode '{code}': active rule is '{this.activeRule?.Action ?? "<none>"}' for context '{activeContext?.ActiveContextName ?? "<global>"}'.");

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
                this.logger.Warn($"Barcode '{code}': did not match the chained rule '{nextRule.Action}'.");

                if (this.activeRule.RestartOnMismatch)
                {
                    this.logger.Warn($"Barcode '{code}': resetting rule chain.");

                    return this.GetActiveContextRule(code, activeContext?.ActiveContextName);
                }
            }

            return this.activeRule;
        }

        private void WmsStatusTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!this.needLoadRules)
            {
                this.needLoadRules = this.oldWmsStatus != this.healthProbeService.HealthWmsStatus;
            }
            this.oldWmsStatus = this.healthProbeService.HealthWmsStatus;
        }

        #endregion
    }
}
