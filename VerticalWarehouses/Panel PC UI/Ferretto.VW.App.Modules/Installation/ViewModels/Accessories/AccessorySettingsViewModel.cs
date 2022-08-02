using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public abstract class AccessorySettingsViewModel : BaseMainViewModel
    {
        #region Fields

        private bool areSettingsChanged;

        private string firmwareVersion;

        private bool isAccessoryEnabled;

        private bool isEnabled;

        private DateTimeOffset? manufactureDate;

        private string modelNumber;

        private DelegateCommand saveCommand;

        private string serialNumber;

        private DelegateCommand testCommand;

        #endregion

        #region Constructors

        protected AccessorySettingsViewModel()
            : base(PresentationMode.Installer)
        {
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
                    if (this.areSettingsChanged)
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

        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (this.SetProperty(ref this.isEnabled, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public DateTimeOffset? ManufactureDate
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

        public System.Windows.Input.ICommand SaveCommand =>
           this.saveCommand
           ??
           (this.saveCommand = new DelegateCommand(
               async () =>
               {
                   try
                   {
                       await this.SaveAsync();
                       this.ShowNotification(VW.App.Resources.InstallationApp.SaveSuccessful, Services.Models.NotificationSeverity.Success);
                       this.AreSettingsChanged = false;
                   }
                   catch (Exception ex)
                   {
                       this.ShowNotification(ex);
                   }
               },
               this.CanSave));

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

        public System.Windows.Input.ICommand TestCommand =>
                   this.testCommand
           ??
           (this.testCommand = new DelegateCommand(
               async () =>
               {
                   try
                   {
                       await this.TestAsync();
                       this.ShowNotification(VW.App.Resources.InstallationApp.CompletedTest, Services.Models.NotificationSeverity.Success);
                       this.AreSettingsChanged = false;
                   }
                   catch (Exception ex)
                   {
                       this.ShowNotification(ex.Message, Services.Models.NotificationSeverity.Error);
                   }
               },
               this.CanTest));

        #endregion

        #region Methods

        public bool CanEnable()
        {
            return
                (this.MachineModeService.MachinePower == MachinePowerState.Powered
                ||
                this.MachineModeService.MachinePower == MachinePowerState.Unpowered)
                &&
                (this.HealthProbeService.HealthMasStatus == HealthStatus.Healthy
                ||
                this.HealthProbeService.HealthMasStatus == HealthStatus.Degraded);
        }

        protected virtual bool CanSave()
        {
            return
                !this.IsWaitingForResponse
                &&
                this.AreSettingsChanged;
        }

        protected virtual bool CanTest()
        {
            return
                !this.IsWaitingForResponse
                &&
                this.IsAccessoryEnabled;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.saveCommand?.RaiseCanExecuteChanged();
            this.testCommand?.RaiseCanExecuteChanged();
        }

        protected abstract Task SaveAsync();

        protected void SetDeviceInformation(MAS.AutomationService.Contracts.DeviceInformation deviceInformation)
        {
            if (deviceInformation is null)
            {
                return;
            }

            this.FirmwareVersion = deviceInformation.FirmwareVersion;
            this.SerialNumber = deviceInformation.SerialNumber;
            this.ManufactureDate = deviceInformation.ManufactureDate;
            this.ModelNumber = deviceInformation.ModelNumber;
        }

        protected abstract Task TestAsync();

        #endregion
    }
}
