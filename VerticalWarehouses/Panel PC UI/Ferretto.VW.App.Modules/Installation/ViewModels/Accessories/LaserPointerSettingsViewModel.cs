using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Xpf.Editors.Helpers;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.LaserPointer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Ferretto.VW.App.Resources;
using Prism.Commands;
using System.Windows.Media;

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

        private string connectionLabel = "Disconnesso";

        private DeviceInformation deviceInformation;

        private IPAddress ipAddress;

        private bool isAccessoryEnabled;

        private LaserPointerDriver laserPointerDriver;

        private int port;

        private DelegateCommand saveCommand;

        private bool testIsChecked;

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

        public DeviceInformation DeviceInformation => this.deviceInformation;

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

                if (!(accessories.LaserPointer.DeviceInformation is null))
                {
                    this.deviceInformation = new DeviceInformation
                    {
                        FirmwareVersion = accessories.LaserPointer.DeviceInformation.FirmwareVersion,
                        Id = accessories.LaserPointer.DeviceInformation.Id,
                        ModelNumber = accessories.LaserPointer.DeviceInformation.ModelNumber,
                        SerialNumber = accessories.LaserPointer.DeviceInformation.SerialNumber,
                        ManufactureDate = accessories.LaserPointer.DeviceInformation.ManufactureDate.TryConvertToDateTime(),
                    };
                }

                this.Port = accessories.LaserPointer.TcpPort;
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
                this.ConnectionBrush = Brushes.DarkOrange;

                this.laserPointerDriver.Configure(this.ipAddress, this.port);
                var result = await this.laserPointerDriver.IsConnectedAsync();

                if (result)
                {
                    this.ConnectionLabel = VW.App.Resources.Localized.Get("InstallationApp.WmsStatusOnline");
                    this.ConnectionBrush = Brushes.Green;
                }
                else
                {
                    this.ConnectionLabel = VW.App.Resources.Localized.Get("InstallationApp.WmsStatusOffline");
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
                this.laserPointerDriver.Configure(this.ipAddress, this.port, this.yOffset, this.zOffsetLowerPosition, this.zOffsetUpperPosition);
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

        #endregion
    }
}
