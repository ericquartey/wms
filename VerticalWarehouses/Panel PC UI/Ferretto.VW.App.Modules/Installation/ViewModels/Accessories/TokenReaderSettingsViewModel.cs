using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.TokenReader;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class TokenReaderSettingsViewModel : AccessorySettingsViewModel
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly ISerialPortsService serialPortsService;

        private readonly ITokenReaderService tokenReaderService;

        private bool isTokenInserted;

        private string isTokenInsertedDescription;

        private string portName;

        private string receivedTokenCode;

        private bool systemPortsAvailable;

        #endregion

        #region Constructors

        public TokenReaderSettingsViewModel(
                    ITokenReaderService tokenReaderService,
            IAuthenticationService authenticationService,
            ISerialPortsService serialPortsService)
        {
            this.tokenReaderService = tokenReaderService;
            this.authenticationService = authenticationService;
            this.serialPortsService = serialPortsService;

            this.serialPortsService.PortNames.CollectionChanged += this.OnPortNamesChanged;
            this.SystemPortsAvailable = this.serialPortsService.PortNames.Any();

            this.OnTokenStatusChangedEventHandler = new EventHandler<TokenStatusChangedEventArgs>(
                this.OnTokenStatusChanged);
        }

        #endregion

        #region Properties

        public bool IsTokenInserted
        {
            get => this.isTokenInserted;
            set => this.SetProperty(
                ref this.isTokenInserted,
                value,
                this.IsTokenInsertedDescription = value ? Localized.Get("General.Present") : Localized.Get("General.NotPresent"));
        }

        public string IsTokenInsertedDescription
        {
            get => this.isTokenInsertedDescription;
            set => this.SetProperty(ref this.isTokenInsertedDescription, value);
        }

        public EventHandler<TokenStatusChangedEventArgs> OnTokenStatusChangedEventHandler { get; }

        public string PortName
        {
            get => this.portName;
            set => this.SetProperty(ref this.portName, value, () => this.AreSettingsChanged = true);
        }

        public IEnumerable<string> PortNames => this.serialPortsService.PortNames;

        public string ReceivedTokenCode
        {
            get => this.receivedTokenCode;
            set => this.SetProperty(ref this.receivedTokenCode, value);
        }

        public bool SystemPortsAvailable
        {
            get => this.systemPortsAvailable;
            set => this.SetProperty(ref this.systemPortsAvailable, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.tokenReaderService.TokenStatusChanged -= this.OnTokenStatusChangedEventHandler;
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.tokenReaderService.TokenStatusChanged += this.OnTokenStatusChangedEventHandler;
            this.IsTokenInserted = this.tokenReaderService.IsTokenInserted;
            this.ReceivedTokenCode = this.tokenReaderService.TokenSerialNumber;
        }

        protected override async Task OnDataRefreshAsync()
        {
            await base.OnDataRefreshAsync();

            try
            {
                this.IsEnabled = this.CanEnable();
                this.RaisePropertyChanged(nameof(this.IsEnabled));

                if (this.Data is BayAccessories bayAccessories
                    &&
                    bayAccessories.TokenReader != null)
                {
                    this.IsAccessoryEnabled = bayAccessories.TokenReader.IsEnabledNew;
                    this.PortName = bayAccessories.TokenReader.PortName;
                }
                else
                {
                    this.Logger.Warn("Improper parameters were passed to the token reader settings page.");
                }

                await this.tokenReaderService.StartAsync();

                this.AreSettingsChanged = false;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected override async Task SaveAsync()
        {
            try
            {
                this.Logger.Debug("Saving token reader settings ...");

                this.ClearNotifications();
                this.IsWaitingForResponse = true;
                await this.tokenReaderService.UpdateSettingsAsync(
                    this.IsAccessoryEnabled,
                    this.PortName);

                this.Logger.Debug("Token reader settings saved.");
            }
            catch
            {
                throw;
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void OnPortNamesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.SystemPortsAvailable = this.serialPortsService.PortNames.Any();
        }

        private void OnTokenStatusChanged(object sender, TokenStatusChangedEventArgs e)
        {
            this.ReceivedTokenCode = e.SerialNumber;
            this.IsTokenInserted = e.IsInserted;
        }

        #endregion
    }
}
