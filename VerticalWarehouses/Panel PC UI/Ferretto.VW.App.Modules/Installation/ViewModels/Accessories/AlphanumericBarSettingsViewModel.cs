using System;
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

        private string firmwareVersion;

        private IPAddress ipAddress;

        private bool isAccessoryEnabled;

        private string manufactureDate;

        private string modelNumber;

        private int port;

        private DelegateCommand saveCommand;

        private string serialNumber;

        private AlphaNumericBarSize size;

        private bool testArrowIsChecked;

        private int testArrowOffset;

        private bool testLedIsChecked;

        private bool testMessageIsChecked;

        private int testMessageOffset;

        private string testMessageText;

        private bool testOffIsChecked;

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

        public string FirmwareVersion
        {
            get => this.firmwareVersion;
            set
            {
                if (this.SetProperty(ref this.firmwareVersion, value))
                {
                    this.AreSettingsChanged = true;
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

        public string ManufactureDate
        {
            get => this.manufactureDate;
            set
            {
                if (this.SetProperty(ref this.manufactureDate, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string ModelNumber
        {
            get => this.modelNumber;
            set
            {
                if (this.SetProperty(ref this.modelNumber, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
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

        public string SerialNumber
        {
            get => this.serialNumber;
            set
            {
                if (this.SetProperty(ref this.serialNumber, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

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

        public bool TestArrowIsChecked
        {
            get => this.testArrowIsChecked;
            set
            {
                if (this.SetProperty(ref this.testArrowIsChecked, value))
                {
                    if (value)
                    {
                        _ = this.DoTestArrowOnAsync(this.testArrowOffset);
                    }
                }
            }
        }

        public int TestArrowOffset
        {
            get => this.testArrowOffset;
            set
            {
                if (value < 0 || value > 5000)
                {
                    value = 0;
                }

                if (this.SetProperty(ref this.testArrowOffset, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool TestLedIsChecked
        {
            get => this.testLedIsChecked;
            set
            {
                if (this.SetProperty(ref this.testLedIsChecked, value))
                {
                    if (value)
                    {
                        _ = this.DoTestLedAsync(value);
                    }
                }
            }
        }

        public bool TestMessageIsChecked
        {
            get => this.testMessageIsChecked;
            set
            {
                if (this.SetProperty(ref this.testMessageIsChecked, value))
                {
                    if (value)
                    {
                        _ = this.DoTestMessageOnAsync(this.TestMessageText, this.testMessageOffset);
                    }
                }
            }
        }

        public int TestMessageOffset
        {
            get => this.testMessageOffset;
            set
            {
                if (value < 0 || value > 5000)
                {
                    value = 0;
                }

                if (this.SetProperty(ref this.testMessageOffset, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public string TestMessageText
        {
            get => this.testMessageText;
            set
            {
                if (this.SetProperty(ref this.testMessageText, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public bool TestOffIsChecked
        {
            get => this.testOffIsChecked;
            set
            {
                if (this.SetProperty(ref this.testOffIsChecked, value))
                {
                    if (value)
                    {
                        _ = this.DoTestLedAsync(false);
                    }
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

                this.IsAccessoryEnabled = accessories.AlphaNumericBar.IsEnabledNew;
                this.IpAddress = accessories.AlphaNumericBar.IpAddress;
                this.Port = accessories.AlphaNumericBar.TcpPort;
                this.Size = (Ferretto.VW.MAS.DataModels.AlphaNumericBarSize)accessories.AlphaNumericBar.Size;

                this.SetDeviceInformation(accessories);

                this.AreSettingsChanged = false;

                if (this.TestMessageText is null)
                {
                    this.TestMessageText = $"{DateTime.Now} {Ferretto.VW.App.Resources.Menu.AccessoriesAlphaNumBarTestMessageDefault}";
                }

                this.TestArrowOffset = this.alphaNumericBarDriver.CalculateOffsetArrowMiddlePosition();
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

        private async Task<bool> DoTestArrowOnAsync(int offset)
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.alphaNumericBarDriver.Configure(this.ipAddress, this.port, this.size);
                if (this.alphaNumericBarDriver.TestEnabled)
                {
                    await this.alphaNumericBarDriver.TestAsync(false);
                }

                return await this.alphaNumericBarDriver.SetCustomCharacterAsync(0, offset, true);
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

        private async Task<bool> DoTestLedAsync(bool enable)
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.alphaNumericBarDriver.Configure(this.ipAddress, this.port, this.size);
                return await this.alphaNumericBarDriver.TestAsync(enable);
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

        private async Task<bool> DoTestMessageOnAsync(string message, int offset)
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.alphaNumericBarDriver.Configure(this.ipAddress, this.port, this.size);
                if (this.alphaNumericBarDriver.TestEnabled)
                {
                    await this.alphaNumericBarDriver.TestAsync(false);
                }

                return await this.alphaNumericBarDriver.SetAndWriteMessageAsync(message, offset, true);
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

                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.SaveSuccessful"), Services.Models.NotificationSeverity.Success);
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

        private void SetDeviceInformation(MAS.AutomationService.Contracts.BayAccessories accessories)
        {
            if (!(accessories.LaserPointer.DeviceInformation is null))
            {
                this.FirmwareVersion = accessories.LaserPointer.DeviceInformation.FirmwareVersion;
                this.ModelNumber = accessories.LaserPointer.DeviceInformation.ModelNumber;
                this.SerialNumber = accessories.LaserPointer.DeviceInformation.SerialNumber;

                if (accessories.LaserPointer.DeviceInformation.ManufactureDate is null)
                {
                    this.ManufactureDate = "-";
                }
                else
                {
                    this.ManufactureDate = accessories.LaserPointer.DeviceInformation.ManufactureDate.ToString();
                }
            }
        }

        #endregion
    }
}
