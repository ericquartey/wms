using System;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.TokenReader;
using Ferretto.VW.MAS.AutomationService.Contracts;
using NLog;

namespace Ferretto.VW.App.Accessories
{
    internal sealed class TokenReaderService : ITokenReaderService, IDisposable
    {
        #region Fields

        private readonly IMachineAccessoriesWebService accessoriesWebService;

        private readonly IAuthenticationService authenticationService;

        private readonly ITokenReaderDriver deviceDriver;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly INavigationService navigationService;

        private bool isDeviceEnabled;

        private bool isDisposed;

        private bool isStarted;

        #endregion

        #region Constructors

        public TokenReaderService(
            IMachineAccessoriesWebService accessoriesWebService,
            ITokenReaderDriver deviceDriver,
            IAuthenticationService authenticationService,
            INavigationService navigationService)
        {
            this.accessoriesWebService = accessoriesWebService;
            this.deviceDriver = deviceDriver;
            this.authenticationService = authenticationService;
            this.navigationService = navigationService;

            this.deviceDriver.TokenStatusChanged += this.OnTokenStatusChanged;
        }

        #endregion

        #region Events

        public event EventHandler<TokenStatusChangedEventArgs> TokenStatusChanged;

        #endregion

        #region Properties

        public Devices.DeviceInformation DeviceInformation => throw new NotSupportedException();

        public bool IsTokenInserted { get; private set; }

        public string TokenSerialNumber { get; private set; }

        #endregion

        #region Methods

        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;
            }
        }

        public async Task StartAsync()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(BarcodeReaderService));
            }

            if (this.isStarted)
            {
                this.logger.Warn("The token reader service is already started.");
                return;
            }

            this.logger.Debug("Starting the token reader service ...");

            try
            {
                var accessories = await this.accessoriesWebService.GetAllAsync();

                this.isDeviceEnabled = accessories.TokenReader?.IsEnabledNew == true;
                if (!this.isDeviceEnabled)
                {
                    this.logger.Debug("The token reader is not configured to be enabled.");
                    return;
                }

                this.deviceDriver.Connect(
                    new TokenReaderSerialPortOptions
                    {
                        PortName = accessories.TokenReader.PortName
                    });

                this.isStarted = true;
                this.logger.Debug("Token reader service started.");
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Error while initializing the token reader service service.");

                throw;
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

        public async Task UpdateSettingsAsync(bool isEnabled, string portName)
        {
            await this.accessoriesWebService.UpdateTokenReaderSettingsAsync(isEnabled, portName);

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

        private void OnTokenStatusChanged(object sender, TokenStatusChangedEventArgs e)
        {
            this.IsTokenInserted = e.IsInserted;
            this.TokenSerialNumber = e.SerialNumber;

            if (!e.IsInserted)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!this.navigationService.IsActiveView(nameof(Utils.Modules.Accessories), Utils.Modules.Accessories.TokenReader))
                    {
                        this.authenticationService.LogOutAsync();
                        this.navigationService.GoBackTo(nameof(Utils.Modules.Login), Utils.Modules.Login.LOGIN);
                    }
                });
            }

            this.TokenStatusChanged?.Invoke(sender, e);
        }

        #endregion
    }
}
