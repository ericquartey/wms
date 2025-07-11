using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class WeightingScaleSettingsViewModel : AccessorySettingsViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly IWeightingScaleService weightingScaleService;

        private SubscriptionToken barcodeSubscriptionToken;

        private DelegateCommand configureDeviceCommand;

        private IPAddress ipAddress;

        private WeightingScaleModelNumber modelNumberCombo;

        private int port;

        #endregion

        #region Constructors

        public WeightingScaleSettingsViewModel(
            IWeightingScaleService weightingScaleService,
            IEventAggregator eventAggregator)
        {
            this.weightingScaleService = weightingScaleService;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public bool IsEnabledEditing => true;

        public ICommand ConfigureDeviceCommand =>
            this.configureDeviceCommand
            ??
            (this.configureDeviceCommand = new DelegateCommand(
                this.ConfigureDevice,
                this.CanConfigureDevice));

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

        public WeightingScaleModelNumber ModelNumberCombo
        {
            get => this.modelNumberCombo;
            set
            {
                if (this.SetProperty(ref this.modelNumberCombo, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        public IEnumerable<WeightingScaleModelNumber> ModelNumbers => Enum.GetValues(typeof(WeightingScaleModelNumber)).OfType<WeightingScaleModelNumber>().ToList();

        public int Port
        {
            get => this.port;
            set
            {
                if (value < IPEndPoint.MinPort || value > IPEndPoint.MaxPort)
                {
                    this.ShowNotification(App.Resources.Localized.Get("OperatorApp.GenericValidationError"), Services.Models.NotificationSeverity.Error);
                    return;
                }

                if (this.SetProperty(ref this.port, value))
                {
                    this.AreSettingsChanged = true;
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.barcodeSubscriptionToken?.Dispose();
            this.barcodeSubscriptionToken = null;
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            //this.barcodeSubscriptionToken =
            //    this.barcodeSubscriptionToken
            //    ??
            //    this.eventAggregator
            //        .GetEvent<PubSubEvent<ActionEventArgs>>()
            //        .Subscribe(
            //            this.OnBarcodeReceived,
            //            ThreadOption.UIThread,
            //            false);
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
                    bayAccessories.WeightingScale != null)
                {
                    this.IsAccessoryEnabled = bayAccessories.WeightingScale.IsEnabledNew;
                    this.IpAddress = bayAccessories.WeightingScale.IpAddress;
                    this.Port = bayAccessories.WeightingScale.TcpPort;
                    this.ModelNumber = bayAccessories.WeightingScale.DeviceInformation?.ModelNumber;
                    this.ModelNumberCombo = this.ModelNumber == WeightingScaleModelNumber.MinebeaIntec.ToString() ? WeightingScaleModelNumber.MinebeaIntec : WeightingScaleModelNumber.DiniArgeo;

                    this.SetDeviceInformation(bayAccessories.WeightingScale.DeviceInformation);
                }
                else
                {
                    this.Logger.Warn("Improper parameters were passed to Weighting Scale settings page.");
                }

                var liveInformation = this.weightingScaleService.DeviceInformation;
                this.FirmwareVersion = liveInformation.FirmwareVersion;
                this.SerialNumber = liveInformation.SerialNumber;
                this.ManufactureDate = liveInformation.ManufactureDate;
                //this.ModelNumber = liveInformation.ModelNumber;

                this.AreSettingsChanged = false;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.ModelNumberCombo));
            this.RaisePropertyChanged(nameof(this.ModelNumbers));
        }

        protected override async Task SaveAsync()
        {
            try
            {
                this.Logger.Debug("Saving Weighting Scale settings ...");
                this.ClearNotifications();
                this.IsWaitingForResponse = true;
                await this.weightingScaleService.UpdateSettingsAsync(this.IsAccessoryEnabled, this.IpAddress.ToString(), this.Port, this.modelNumberCombo);

                this.Logger.Debug("Weighting Scale settings saved.");

                var liveInformation = this.weightingScaleService.DeviceInformation;
                this.FirmwareVersion = liveInformation.FirmwareVersion;
                this.SerialNumber = liveInformation.SerialNumber;
                this.ManufactureDate = liveInformation.ManufactureDate;
                this.ModelNumber = liveInformation.ModelNumber;
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

        protected override async Task TestAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                //Do nothing
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

        private bool CanConfigureDevice()
        {
            return !this.IsWaitingForResponse;
        }

        private void ConfigureDevice()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Accessories.BarcodeReaderConfig);
        }

        #endregion
    }
}
