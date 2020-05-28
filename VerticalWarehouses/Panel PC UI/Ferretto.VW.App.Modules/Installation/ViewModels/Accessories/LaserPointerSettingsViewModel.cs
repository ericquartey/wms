using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.LaserPointer;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal class LaserPointerSettingsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private bool areSettingsChanged;

        private DelegateCommand checkConnectionCommand;

        private Brush connectionBrush;

        private string connectionLabel = VW.App.Resources.Localized.Get("InstallationApp.WmsStatusOffline");

        private string firmwareVersion;

        private IPAddress ipAddress;

        private bool isAccessoryEnabled;

        private LaserPointerDriver laserPointerDriver;

        private string manufactureDate;

        private string modelNumber;

        private int port;

        private DelegateCommand saveCommand;

        private string serialNumber;

        private bool testIsChecked;

        private double xOffset;

        private double yOffset;

        private double zOffsetLowerPosition;

        private double zOffsetUpperPosition;

        #endregion

        #region Constructors

        public LaserPointerSettingsViewModel(IBayManager bayManager)
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

        public ICommand CheckConnectionCommand =>
            this.checkConnectionCommand
            ??
            (this.checkConnectionCommand = new DelegateCommand(
            async () => await this.CheckConnectionAsync(), this.CanCheckConnection));

        public Brush ConnectionBrush
        {
            get => this.connectionBrush;
            set => this.SetProperty(ref this.connectionBrush, value);
        }

        public string ConnectionLabel
        {
            get => this.connectionLabel;
            set
            {
                this.SetProperty(ref this.connectionLabel, value);
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
                    value = LaserPointerDriver.PORT_DEFAULT;
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

        public double XOffset
        {
            get => this.xOffset;
            set
            {
                if (this.SetProperty(ref this.xOffset, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public double YOffset
        {
            get => this.yOffset;
            set
            {
                if (this.SetProperty(ref this.yOffset, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public double ZOffsetLowerPosition
        {
            get => this.zOffsetLowerPosition;
            set
            {
                if (this.SetProperty(ref this.zOffsetLowerPosition, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public double ZOffsetUpperPosition
        {
            get => this.zOffsetUpperPosition;
            set
            {
                if (this.SetProperty(ref this.zOffsetUpperPosition, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
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

                if (this.laserPointerDriver is null)
                {
                    this.laserPointerDriver = new LaserPointerDriver();
                }

                this.IsAccessoryEnabled = accessories.LaserPointer.IsEnabledNew;
                this.IpAddress = accessories.LaserPointer.IpAddress;
                this.Port = accessories.LaserPointer.TcpPort;

                this.SetDeviceInformation(accessories);
                this.YOffset = (int)accessories.LaserPointer.YOffset;
                this.ZOffsetUpperPosition = (int)accessories.LaserPointer.ZOffsetUpperPosition;
                this.ZOffsetLowerPosition = (int)accessories.LaserPointer.ZOffsetLowerPosition;

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

        private bool CanCheckConnection()
        {
            return !this.IsWaitingForResponse;
        }

        private bool CanSave()
        {
            return !this.IsWaitingForResponse;
        }

        private async Task CheckConnectionAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.ConnectionLabel = VW.App.Resources.Localized.Get("InstallationApp.Wait");
                this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.Wait"), Services.Models.NotificationSeverity.Warning);
                this.ConnectionBrush = Brushes.DarkOrange;

                this.laserPointerDriver.Configure(this.ipAddress, this.port);
                var result = await this.laserPointerDriver.IsConnectedAsync();

                if (result)
                {
                    this.ConnectionLabel = VW.App.Resources.Localized.Get("InstallationApp.WmsStatusOnline");
                    this.ConnectionBrush = Brushes.Green;
                    this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.WmsStatusOnline"), Services.Models.NotificationSeverity.Success);
                }
                else
                {
                    this.ConnectionLabel = VW.App.Resources.Localized.Get("InstallationApp.WmsStatusOffline");
                    this.ShowNotification(VW.App.Resources.Localized.Get("InstallationApp.WmsStatusOffline"), Services.Models.NotificationSeverity.Error);
                    this.ConnectionBrush = Brushes.Red;
                }
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

        private async Task<bool> DoTestAsync(bool enable)
        {
            try
            {
                this.IsWaitingForResponse = true;
                this.laserPointerDriver.Configure(this.ipAddress, this.port, this.xOffset, this.yOffset, this.zOffsetLowerPosition, this.zOffsetUpperPosition);
                return await this.laserPointerDriver.TestAsync(enable);
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
                await this.bayManager.SetSetLaserPointerAsync(this.IsAccessoryEnabled, this.ipAddress, this.port, this.yOffset, this.zOffsetLowerPosition, this.zOffsetUpperPosition);
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
