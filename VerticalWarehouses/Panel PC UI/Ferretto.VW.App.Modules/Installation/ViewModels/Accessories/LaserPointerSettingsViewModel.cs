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
    internal class LaserPointerSettingsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private bool areSettingsChanged;

        private IPAddress ipAddress;

        private bool isAccessoryEnabled;

        private LaserPointer laserPointer;

        private int port;

        private DelegateCommand saveCommand;

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

                if (this.laserPointer is null)
                {
                    this.laserPointer = new LaserPointer();
                }

                this.IsAccessoryEnabled = accessories.AlphaNumericBar.IsEnabledNew;
                this.IpAddress = accessories.AlphaNumericBar.IpAddress;
                this.Port = accessories.AlphaNumericBar.TcpPort;
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

        #endregion
    }
}
