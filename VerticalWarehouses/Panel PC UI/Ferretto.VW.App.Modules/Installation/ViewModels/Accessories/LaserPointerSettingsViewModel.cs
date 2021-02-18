using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.LaserPointer;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal class LaserPointerSettingsViewModel : AccessorySettingsViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly ILaserPointerDriver deviceDriver;

        private DelegateCommand checkConnectionCommand;

        private Brush connectionBrush;

        private string connectionLabel = VW.App.Resources.Localized.Get("InstallationApp.WmsStatusOffline");

        private IPAddress ipAddress;

        private int port;

        private bool testIsChecked;

        private double xOffset;

        private double yOffset;

        private double zOffsetLowerPosition;

        private double zOffsetUpperPosition;

        #endregion

        #region Constructors

        public LaserPointerSettingsViewModel(
            IBayManager bayManager,
            ILaserPointerDriver laserPointerDriver)
        {
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.deviceDriver = laserPointerDriver;
        }

        #endregion

        #region Properties

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
            set => this.SetProperty(ref this.connectionLabel, value);
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
                this.IsEnabled = this.CanEnable();
                this.RaisePropertyChanged(nameof(this.IsEnabled));

                if (this.Data is BayAccessories accessories)
                {
                    this.IsAccessoryEnabled = accessories.LaserPointer.IsEnabledNew;
                    this.IpAddress = accessories.LaserPointer.IpAddress;
                    this.Port = accessories.LaserPointer.TcpPort;

                    this.YOffset = accessories.LaserPointer.YOffset;
                    this.XOffset = accessories.LaserPointer.XOffset;
                    this.ZOffsetUpperPosition = accessories.LaserPointer.ZOffsetUpperPosition;
                    this.ZOffsetLowerPosition = accessories.LaserPointer.ZOffsetLowerPosition;

                    this.SetDeviceInformation(accessories.LaserPointer.DeviceInformation);

                    this.AreSettingsChanged = false;
                }
                else
                {
                    this.Logger.Warn("Improper parameters were passed to the laser settings page. Leaving the page ...");

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

        protected override async Task SaveAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.bayManager.SetLaserPointerAsync(this.IsAccessoryEnabled, this.ipAddress, this.port, this.xOffset, this.yOffset, this.zOffsetLowerPosition, this.zOffsetUpperPosition);
                this.deviceDriver.Configure(this.ipAddress, this.port, this.xOffset, this.yOffset, this.zOffsetLowerPosition, this.zOffsetUpperPosition);
                if (this.IsAccessoryEnabled)
                {
                    await this.deviceDriver.ConnectAsync(this.ipAddress, this.port);
                }
                else
                {
                    this.deviceDriver.Disconnect();
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
        }

        private bool CanCheckConnection()
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

                this.deviceDriver.Configure(this.ipAddress, this.port);
                var result = await this.deviceDriver.ParametersAsync();

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
            catch (Exception ex)
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
                this.deviceDriver.Configure(this.ipAddress, this.port, this.xOffset, this.yOffset, this.zOffsetLowerPosition, this.zOffsetUpperPosition);
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

        #endregion
    }
}
