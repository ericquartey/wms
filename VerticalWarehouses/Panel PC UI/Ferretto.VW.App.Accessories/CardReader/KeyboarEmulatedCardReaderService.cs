using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Accessories
{
    internal sealed class KeyboarEmulatedCardReaderService : Interfaces.ICardReaderService, IDisposable
    {
        #region Fields

        private static readonly Regex DefaultTokenRegex = new Regex(".+", RegexOptions.Compiled);

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly MAS.AutomationService.Contracts.IMachineBaysWebService machineBaysWebService;

        private readonly StringBuilder stringBuilder = new StringBuilder();

        private readonly TextCompositionEventHandler textCompositionEventHandler;

        private bool isDisposed = false;

        private bool isStarted;

        private Regex tokenRegex;

        #endregion

        #region Constructors

        public KeyboarEmulatedCardReaderService(
            IEventAggregator eventAggregator,
            MAS.AutomationService.Contracts.IMachineBaysWebService machineBaysWebService)
        {
            this.eventAggregator = eventAggregator;
            this.machineBaysWebService = machineBaysWebService;

            this.textCompositionEventHandler = new TextCompositionEventHandler(this.MainWindow_PreviewTextInput);
            Application.Current.MainWindow.PreviewTextInput += this.textCompositionEventHandler;
        }

        #endregion

        #region Events

        public event EventHandler<string> TokenAcquired;

        #endregion

        #region Properties

        public DeviceInformation DeviceInformation => throw new NotSupportedException();

        #endregion

        #region Methods

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing).
            this.Dispose(true);
        }

        public async Task StartAsync()
        {
            if (this.isStarted)
            {
                return;
            }

            try
            {
                var accessories = await this.machineBaysWebService.GetAccessoriesAsync();
                var isCardReaderEnabled = accessories.CardReader?.IsEnabledNew == true;

                if (isCardReaderEnabled)
                {
                    if (accessories.CardReader.TokenRegex != null)
                    {
                        this.tokenRegex = new Regex(
                            "^.+\rò(?<Token>.+)-\rj");//accessories.CardReader.TokenRegex;
                    }
                    else
                    {
                        this.tokenRegex = DefaultTokenRegex;
                    }

                    this.isStarted = true;
                    this.logger.Debug("Starting keyboard-emulated card reader service.");
                }
            }
            catch
            {
                this.NotifyError("Unable to determine whether the card reader is enabled or not.");
            }
        }

        public Task StopAsync()
        {
            if (this.isStarted)
            {
                this.logger.Debug("Stopping keyboard-emulated card reader service.");
                this.isStarted = false;
            }

            return Task.CompletedTask;
        }

        private void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    Application.Current.MainWindow.PreviewTextInput -= this.textCompositionEventHandler;
                }

                this.isDisposed = true;
            }
        }

        private void MainWindow_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!this.isStarted)
            {
                return;
            }

            this.stringBuilder.Append(e.Text);

            var match = this.tokenRegex.Match(this.stringBuilder.ToString());

            if (match.Groups["Token"].Success)
            {
                this.logger.Debug($"Found match '{this.stringBuilder}'.");

                var token = match.Groups["Token"].Value;

                this.logger.Debug($"Found token '{token}'.");

                this.TokenAcquired?.Invoke(this, token);
                this.stringBuilder.Clear();
            }

            e.Handled = true;
        }

        private void NotifyError(string message)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(message, Services.Models.NotificationSeverity.Error));
        }

        #endregion

        // To detect redundant calls
        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~KeyboarEmulatedCardReaderService()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }
        /*
                private void OnMainWindowKeyDown(object sender, KeyEventArgs e)
                {
                    var inputKey = new KeyConverter().ConvertToInvariantString(e.Key);

                    if (this.tokenRegex is null)
                    {
                        if (e.Key is Key.Return)
                        {
                            this.logger.Debug($"Received token '{this.stringBuilder}'.");

                            this.TokenAcquired?.Invoke(this, this.stringBuilder.ToString());
                            this.stringBuilder.Clear();
                        }
                        else
                        {
                            this.stringBuilder.Append(inputKey);
                        }
                    }
                    else
                    {
                        this.stringBuilder.Append(inputKey);

                        var match = this.tokenRegex.Match(this.stringBuilder.ToString());
                        if (match.Groups["Token"].Success)
                        {
                            this.logger.Debug($"Found match '{this.stringBuilder}'.");

                            var token = match.Groups["Token"].Value;

                            this.TokenAcquired?.Invoke(this, token);
                            this.stringBuilder.Clear();
                        }
                    }

                    e.Handled = true;
                }*/
    }
}
