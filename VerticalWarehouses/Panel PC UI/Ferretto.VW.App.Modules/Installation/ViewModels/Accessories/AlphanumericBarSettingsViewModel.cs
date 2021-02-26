using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.AlphaNumericBar;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal class AlphaNumericBarSettingsViewModel : AccessorySettingsViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IAlphaNumericBarDriver deviceDriver;

        private readonly IAlphaNumericBarService deviceService;

        private IPAddress ipAddress;

        private int port;

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

        public AlphaNumericBarSettingsViewModel(
            IBayManager bayManager,
            IAlphaNumericBarDriver deviceDriver,
            IAlphaNumericBarService alphaNumericBarService)
        {
            this.bayManager = bayManager;
            this.deviceDriver = deviceDriver;
            this.deviceService = alphaNumericBarService;
        }

        #endregion

        #region Properties

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

        public bool IsEnabledEditing => true;

        public int Port
        {
            get => this.port;
            set
            {
                if (value < IPEndPoint.MinPort || value > IPEndPoint.MaxPort)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                if (this.SetProperty(ref this.port, value))
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

        public IEnumerable<AlphaNumericBarSize> Sizes => Enum.GetValues(typeof(AlphaNumericBarSize)).OfType<AlphaNumericBarSize>().ToList();

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
                this.IsEnabled = this.CanEnable();
                this.RaisePropertyChanged(nameof(this.IsEnabled));

                if (this.Data is BayAccessories bayAccessories)
                {
                    this.IsWaitingForResponse = true;

                    this.IsAccessoryEnabled = bayAccessories.AlphaNumericBar.IsEnabledNew;
                    this.IpAddress = bayAccessories.AlphaNumericBar.IpAddress;
                    this.Port = bayAccessories.AlphaNumericBar.TcpPort;
                    this.Size = bayAccessories.AlphaNumericBar.Size;
                    await this.deviceService.AlphaNumericBarConfigureAsync();
                    //this.deviceDriver.Configure(this.ipAddress, this.port, (MAS.DataModels.AlphaNumericBarSize)this.size);
                    //if (this.IsAccessoryEnabled)
                    //{
                    //    await this.deviceDriver.ConnectAsync();
                    //}
                    //else
                    //{
                    //    this.deviceDriver.Disconnect();
                    //}

                    this.SetDeviceInformation(bayAccessories.AlphaNumericBar.DeviceInformation);

                    this.AreSettingsChanged = false;

                    if (this.TestMessageText is null)
                    {
                        this.TestMessageText = $"{DateTime.Now} {Ferretto.VW.App.Resources.Menu.AccessoriesAlphaNumBarTestMessageDefault}";
                    }

                    this.TestArrowOffset = this.deviceDriver.CalculateOffsetArrowMiddlePosition();
                }
                else
                {
                    this.Logger.Warn("Improper parameters were passed to the alphanumeric settings page. Leaving the page ...");

                    this.NavigationService.GoBack();
                }
            }
            catch (Exception ex)
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
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.Size));
            this.RaisePropertyChanged(nameof(this.Sizes));
            this.RaisePropertyChanged(nameof(this.IsEnabledEditing));
        }

        protected override async Task SaveAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.bayManager.SetAlphaNumericBarAsync(this.IsAccessoryEnabled, this.ipAddress, this.port);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task<bool> DoTestArrowOnAsync(int offset)
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.deviceDriver.Configure(this.ipAddress, this.port, this.size);
                await this.deviceDriver.EnabledAsync(false);
                if (this.deviceDriver.TestEnabled)
                {
                    await this.deviceDriver.TestAsync(false);
                }

                return await this.deviceDriver.SetCustomCharacterAsync(0, offset, true);
            }
            catch (Exception ex)
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
                this.deviceDriver.Configure(this.ipAddress, this.port, this.size);
                return await this.deviceDriver.TestAsync(enable);
            }
            catch (Exception ex)
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
                this.deviceDriver.Configure(this.ipAddress, this.port, this.size);

                this.Logger.Debug($"DoTestMessageOnAsync; message {message}");

                await this.deviceDriver.EnabledAsync(false);

                if (this.deviceDriver.TestEnabled)
                {
                    await this.deviceDriver.TestAsync(false);
                }

                this.deviceDriver.GetOffsetArrowAndMessage(offset, message, out var offsetArrow, out var offsetMessage);
                await this.deviceDriver.SetAndWriteArrowAsync(offsetArrow, true);
                return await this.deviceDriver.SetAndWriteMessageAsync(message, offsetMessage, false);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }

            return false;
        }

        #endregion
    }
}
