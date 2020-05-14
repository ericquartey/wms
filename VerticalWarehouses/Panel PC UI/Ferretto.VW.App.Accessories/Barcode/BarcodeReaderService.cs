using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices;
using Ferretto.VW.Devices.BarcodeReader;
using Prism.Events;

namespace Ferretto.VW.App.Accessories
{
    internal sealed class BarcodeReaderService : IBarcodeReaderService, IDisposable
    {
        #region Fields

        private const int SerialPortRefreshInterval = 5000;

        private readonly MAS.AutomationService.Contracts.IMachineBarcodesWebService barcodesWebService;

        private readonly IEventAggregator eventAggregator;

        private readonly MAS.AutomationService.Contracts.IMachineBaysWebService machineBaysWebService;

        private readonly INavigationService navigationService;

        private readonly ObservableCollection<string> portNames = new ObservableCollection<string>();

        private readonly IBarcodeReaderDriver reader;

        private bool isDisposed;

        private bool isStarted;

        private IEnumerable<MAS.AutomationService.Contracts.BarcodeRule> ruleSet;

        private Timer timer;

        #endregion

        #region Constructors

        public BarcodeReaderService(
            IEventAggregator eventAggregator,
            MAS.AutomationService.Contracts.IMachineBaysWebService machineBaysWebService,
            IBarcodeReaderDriver reader,
            INavigationService navigationService,
            MAS.AutomationService.Contracts.IMachineBarcodesWebService barcodesWebService)
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

        public DeviceInformation DeviceInformation
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

        public ObservableCollection<string> PortNames => this.portNames;

        #endregion

        #region Methods

        public void Disable()
        {
            try
            {
                this.reader.Disconnect();

                this.timer?.Dispose();
                this.timer = null;
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

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(true);

            this.Disable();
        }

        public async Task StartAsync()
        {
            if (this.isStarted)
            {
                return;
            }

            try
            {
                this.timer?.Dispose();
                this.timer = new Timer(this.RefreshSystemPorts, null, 0, SerialPortRefreshInterval);

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
            }
            catch (Exception ex)
            {
                this.isStarted = false;
                this.NotifyError(ex);
            }
        }

        private void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            this.isDisposed = true;
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

        private MAS.AutomationService.Contracts.BarcodeRule GetActiveContextRule(string barcode, string activeContextName)
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
                   .Publish(new PresentationNotificationMessage(string.Format(Resources.Localized.Get("OperatorApp.CurrentPageDoesNotSupportBarcodeScanning"), code), Services.Models.NotificationSeverity.Warning));

                System.Diagnostics.Debug.WriteLine($"Current view model does not specify an operational context.");
                return;
            }

            await this.LoadRuleSetAsync();

            var rule = this.GetActiveContextRule(e.Code, activeContext.ActiveContextName);

            if (rule is null)
            {
                this.eventAggregator
                   .GetEvent<PresentationNotificationPubSubEvent>()
                   .Publish(new PresentationNotificationMessage(string.Format(Resources.Localized.Get("OperatorApp.BarcodeNotRecognized"), code), Services.Models.NotificationSeverity.Warning));

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

        private void RefreshSystemPorts(object state)
        {
            var systemPorts = System.IO.Ports.SerialPort.GetPortNames();

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var systemPort in systemPorts)
                {
                    if (!this.portNames.Contains(systemPort))
                    {
                        this.portNames.Add(systemPort);
                    }
                }

                foreach (var knownPort in this.portNames)
                {
                    if (!systemPorts.Contains(knownPort))
                    {
                        this.portNames.Remove(knownPort);
                    }
                }
            });
        }

        #endregion
    }
}
