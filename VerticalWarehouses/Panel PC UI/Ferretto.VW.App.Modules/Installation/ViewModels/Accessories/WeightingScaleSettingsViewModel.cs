using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal class WeightingScaleSettingsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private bool areSettingsChanged;

        private Brush connectionBrush;

        private string firmwareVersion;

        private IPAddress ipAddress;

        private bool isAccessoryEnabled;

        private string manufactureDate;

        private string modelNumber;

        private int port;

        private DelegateCommand saveCommand;

        private string serialNumber;

        #endregion

        #region Constructors

        public WeightingScaleSettingsViewModel(IBayManager bayManager)
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

        public Brush ConnectionBrush
        {
            get => this.connectionBrush;
            set => this.SetProperty(ref this.connectionBrush, value);
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

        private async Task SaveAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

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
