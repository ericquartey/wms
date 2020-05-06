using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.AlphaNumericBar;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal class AlphaNumericBarSettingsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private AlphaNumericBarDriver alphaNumericBarDriver;

        private bool areSettingsChanged;

        private IPAddress ipAddress;

        private bool isAccessoryEnabled;

        private int luminosity = 7;

        private int port;

        private DelegateCommand saveCommand;

        private AlphaNumericBarSize size;

        private bool switchOnIsChecked;

        private bool testIsChecked;

        #endregion

        #region Constructors

        public AlphaNumericBarSettingsViewModel(IBayManager bayManager)
            : base(PresentationMode.Installer)
        {
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
        }

        #endregion

        #region Properties

        public bool AreSettingsChanged
        {
            get => this.areSettingsChanged;
            set
            {
                if (this.SetProperty(ref this.areSettingsChanged, value))
                {
                    if (this.AreSettingsChanged)
                    {
                        this.ClearNotifications();
                    }

                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public IPAddress IpAddress
        {
            get => this.ipAddress;
            set
            {
                if (this.SetProperty(ref this.ipAddress, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsAccessoryEnabled
        {
            get => this.isAccessoryEnabled;
            set
            {
                if (this.SetProperty(ref this.isAccessoryEnabled, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsEnabledEditing => true;

        public int Luminosity
        {
            get => this.luminosity;
            set
            {
                if (this.SetProperty(ref this.luminosity, value))
                {
                    _ = this.DoLuminosityAsync(value);
                }
            }
        }

        public int Port
        {
            get => this.port;
            set
            {
                if (value < 1 || value > 65535)
                {
                    value = AlphaNumericBarDriver.PORT_DEFAULT;
                }

                if (this.SetProperty(ref this.port, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand SaveCommand =>
            this.saveCommand
            ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.SaveAsync(), this.CanSave));

        public AlphaNumericBarSize Size
        {
            get => this.size;
            set
            {
                if (this.SetProperty(ref this.size, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool SwitchOnIsChecked
        {
            get => this.switchOnIsChecked;
            set
            {
                if (this.SetProperty(ref this.switchOnIsChecked, value))
                {
                    _ = this.DoSwitchOnAsync(value);
                }
            }
        }

        public bool TestIsChecked
        {
            get => this.testIsChecked;
            set
            {
                if (this.SetProperty(ref this.testIsChecked, value))
                {
                    _ = this.DoTestAsync(value);
                }
            }
        }

        #endregion

        #region Methods

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                var accessories = await this.bayManager.GetBayAccessoriesAsync();

                if (accessories is null)
                {
                    this.IsAccessoryEnabled = false;
                    return;
                }

                if (this.alphaNumericBarDriver is null)
                {
                    this.alphaNumericBarDriver = new AlphaNumericBarDriver();
                }

                this.IsAccessoryEnabled = accessories.AlphaNumericBar.IsEnabled == "true";
                this.IpAddress = accessories.AlphaNumericBar.IpAddress;
                this.Port = accessories.AlphaNumericBar.TcpPort;
                this.Size = (Ferretto.VW.MAS.DataModels.AlphaNumericBarSize)accessories.AlphaNumericBar.Size;
                this.AreSettingsChanged = false;
            }
            catch (Exception ex) when (ex is MAS.AutomationService.Contracts.MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }

            await base.OnDataRefreshAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.saveCommand?.RaiseCanExecuteChanged();
            base.RaiseCanExecuteChanged();
        }

        private bool CanSave()
        {
            return !this.IsWaitingForResponse;
        }

        private async Task<bool> DoLuminosityAsync(int luminosity)
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.alphaNumericBarDriver.Configure(this.ipAddress, this.port, this.size);
                return await this.alphaNumericBarDriver.SetLuminosityAsync(luminosity);
            }
            catch (Exception ex) when (ex is MAS.AutomationService.Contracts.MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }

            return false;
        }

        private async Task<bool> DoSwitchOnAsync(bool switchOn)
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.alphaNumericBarDriver.Configure(this.ipAddress, this.port, this.size);
                return await this.alphaNumericBarDriver.SetEnabledAsync(switchOn);
            }
            catch (Exception ex) when (ex is MAS.AutomationService.Contracts.MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }

            return false;
        }

        private async Task<bool> DoTestAsync(bool enable)
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.alphaNumericBarDriver.Configure(this.ipAddress, this.port, this.size);
                return await this.alphaNumericBarDriver.SetTestAsync(enable);
            }
            catch (Exception ex) when (ex is MAS.AutomationService.Contracts.MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }

            return false;
        }

        private async Task SaveAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.bayManager.SetAlphaNumericBarAsync(this.IsAccessoryEnabled, this.ipAddress, this.port);

                this.ShowNotification(VW.App.Resources.InstallationApp.SaveSuccessful, Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex) when (ex is MAS.AutomationService.Contracts.MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
