using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Accessories
{
    internal sealed class KeyboarEmulatedCardReaderService : ICardReaderService, IDisposable
    {
        #region Fields

        private const string TokenCaptureGroupName = "Token";

        private static readonly Regex DefaultTokenRegex = new Regex("(?<Token>.+)", RegexOptions.Compiled);

        private readonly IMachineAccessoriesWebService accessoriesWebService;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly StringBuilder stringBuilder = new StringBuilder();

        private readonly TextCompositionEventHandler textCompositionEventHandler;

        private bool isDeviceEnabled;

        private bool isDisposed;

        private bool isStarted;

        private Regex tokenRegex = DefaultTokenRegex;

        #endregion

        #region Constructors

        public KeyboarEmulatedCardReaderService(
            IEventAggregator eventAggregator,
            IMachineAccessoriesWebService accessoriesWebService)
        {
            this.eventAggregator = eventAggregator;
            this.accessoriesWebService = accessoriesWebService;

            this.textCompositionEventHandler = new TextCompositionEventHandler(this.MainWindow_PreviewTextInput);
            Application.Current.MainWindow.PreviewTextInput += this.textCompositionEventHandler;
        }

        #endregion

        #region Events

        /// <summary>
        /// used by test view
        /// </summary>
        public event EventHandler<string> KeysAcquired;

        public event EventHandler<RegexMatchEventArgs> TokenAcquired;

        #endregion

        #region Properties

        public Devices.DeviceInformation DeviceInformation => throw new NotSupportedException();

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
                this.stringBuilder.Clear();

                var accessories = await this.accessoriesWebService.GetAllAsync();

                this.isDeviceEnabled = accessories.CardReader?.IsEnabledNew == true;
                if (accessories.CardReader?.TokenRegex != null)
                {
                    this.tokenRegex = accessories.CardReader?.TokenRegex is null
                        ? DefaultTokenRegex
                        : new Regex(accessories.CardReader.TokenRegex);
                }

                if (this.isDeviceEnabled)
                {
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

        public async Task UpdateSettingsAsync(bool isEnabled, string tokenRegex)
        {
            try
            {
                await this.accessoriesWebService.UpdateCardReaderSettingsAsync(isEnabled, tokenRegex);

                this.tokenRegex = tokenRegex is null
                    ? DefaultTokenRegex
                    : new Regex(tokenRegex);

                this.stringBuilder.Clear();
                this.isDeviceEnabled = isEnabled;

                if (this.isStarted && !this.isDeviceEnabled)
                {
                    await this.StopAsync();
                }
            }
            catch (Exception ex)
            {
                this.NotifyError(ex.Message);
            }
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
            this.logger.Trace($"card reading '{this.stringBuilder}'");

            this.KeysAcquired?.Invoke(this, e.Text);

            var match = this.tokenRegex.Match(this.stringBuilder.ToString());

            if (match.Groups[TokenCaptureGroupName].Success)
            {
                var token = match.Groups[TokenCaptureGroupName].Value;

                this.logger.Debug($"Found token '{token}'.");

                this.TokenAcquired?.Invoke(
                    this,
                    new RegexMatchEventArgs(token, match.Groups[TokenCaptureGroupName].Index));

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
    }
}
