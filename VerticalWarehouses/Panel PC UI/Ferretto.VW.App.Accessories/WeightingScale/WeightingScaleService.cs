using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.App.Accessories.Interfaces.WeightingScale;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
//using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.Devices.WeightingScale;
using Ferretto.VW.MAS.AutomationService.Contracts;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Accessories
{
    internal sealed partial class WeightingScaleService : Interfaces.IWeightingScaleService
    {
        #region Fields

        private const int WeightPollInterval = 600;

        private readonly IMachineAccessoriesWebService accessoriesWebService;

        private readonly IWeightingScaleDriver deviceDriverDini;

        private readonly IWeightingScaleDriver deviceDriverMinebea;

        private readonly Devices.DeviceInformation deviceInformation = new Devices.DeviceInformation();

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly IMachineItemsWebService machineItemsWebService;

        private readonly Timer weightPollTimer;

        private IWeightingScaleDriver deviceDriver;

        private bool isDeviceEnabled;

        private bool isStarted;

        #endregion

        #region Constructors

        public WeightingScaleService(
            IWeightingScaleDriverDini deviceDriverDini,
            IWeightingScaleDriverMinebea deviceDriverMinebea,
            IMachineAccessoriesWebService accessoriesWebService,
            IMachineItemsWebService machineItemsWebService,
            IEventAggregator eventAggregator)
        {
            this.deviceDriverDini = deviceDriverDini;
            this.deviceDriverMinebea = deviceDriverMinebea;
            this.deviceDriver = deviceDriverDini;
            this.accessoriesWebService = accessoriesWebService;
            this.machineItemsWebService = machineItemsWebService;
            this.eventAggregator = eventAggregator;

            this.weightPollTimer = new Timer(async s => await this.OnWeightPollTimerTickAsync());
        }

        #endregion

        #region Events

        public event EventHandler<WeightAcquiredEventArgs> WeighAcquired;

        #endregion

        #region Properties

        public Devices.DeviceInformation DeviceInformation => this.deviceInformation;

        public float UnitaryWeight { get; private set; }

        #endregion

        #region Methods

        public async Task ClearMessageAsync()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WeightingScaleService));
            }

            if (!this.isDeviceEnabled)
            {
                throw new InvalidOperationException(
                    "Cannot perform the operation because the weighting scale service is not enabled.");
            }

            await this.deviceDriver.ClearMessageAsync();
        }

        public async Task DisplayMessageAsync(string message)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WeightingScaleService));
            }

            if (!this.isDeviceEnabled)
            {
                throw new InvalidOperationException(
                    "Cannot perform the operation because the weighting scale service is not enabled.");
            }

            await this.deviceDriver.DisplayMessageAsync(message);
        }

        public async Task DisplayMessageAsync(string message, TimeSpan duration)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WeightingScaleService));
            }

            if (!this.isDeviceEnabled)
            {
                throw new InvalidOperationException(
                    "Cannot perform the operation because the weighting scale service is not enabled.");
            }

            await this.deviceDriver.DisplayMessageAsync(message, duration);
        }

        public async Task<IWeightSample> MeasureWeightAsync(bool poll)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WeightingScaleService));
            }

            if (!this.isDeviceEnabled)
            {
                throw new InvalidOperationException(
                    "Cannot perform the operation because the weighting scale service is not enabled.");
            }

            if (this.weightPollTimer != null)
            {
                this.logger.Debug("Called weight measurement while the continuous polling mode is active.");
            }

            return await this.deviceDriver.MeasureWeightAsync(poll);
        }

        public async Task ResetAverageUnitaryWeightAsync()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WeightingScaleService));
            }

            this.UnitaryWeight = 0;
            //if (!this.isDeviceEnabled)
            //{
            //    throw new InvalidOperationException(
            //        "Cannot perform the operation because the weighting scale service is not enabled.");
            //}

            //await this.deviceDriver.ResetAverageUnitaryWeightAsync();
        }

        public async Task SetAverageUnitaryWeightAsync(float weight)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WeightingScaleService));
            }

            this.UnitaryWeight = weight;

            //if (!this.isDeviceEnabled)
            //{
            //    throw new InvalidOperationException(
            //        "Cannot perform the operation because the weighting scale service is not enabled.");
            //}

            //await this.deviceDriver.SetAverageUnitaryWeightAsync(weight);
        }

        public async Task StartAsync()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WeightingScaleService));
            }

            if (this.isStarted)
            {
                this.logger.Warn("The weighting scale service is already started.");
                return;
            }

            this.isStarted = true;

            this.logger.Info("Starting the weighting scale service ...");

            try
            {
                var accessories = await this.accessoriesWebService.GetAllAsync();
                this.isDeviceEnabled = accessories.WeightingScale?.IsEnabledNew == true;
                if (!this.isDeviceEnabled)
                {
                    this.isStarted = false;
                    throw new InvalidOperationException(
                        "Cannot perform the operation because the weighting scale service is not enabled.");
                }

                // set the device driver according to configuration
                this.deviceDriver = accessories.WeightingScale.DeviceInformation?.ModelNumber == WeightingScaleModelNumber.MinebeaIntec.ToString() ? this.deviceDriverMinebea : this.deviceDriverDini;
                this.logger.Debug($"Weighting scale driver configured as {accessories.WeightingScale.DeviceInformation?.ModelNumber}.");

                await this.deviceDriver.ConnectAsync(accessories.WeightingScale.IpAddress, accessories.WeightingScale.TcpPort);

                this.deviceInformation.FirmwareVersion = await this.deviceDriver.RetrieveVersionAsync();

                if (!string.IsNullOrEmpty(this.deviceInformation.FirmwareVersion))
                {
                    await this.accessoriesWebService.UpdateWeightingScaleDeviceInfoAsync(
                        new DeviceInformation
                        {
                            FirmwareVersion = this.DeviceInformation.FirmwareVersion,
                            ManufactureDate = this.DeviceInformation.ManufactureDate,
                            ModelNumber = accessories.WeightingScale.DeviceInformation?.ModelNumber,
                            SerialNumber = this.DeviceInformation.SerialNumber
                        });
                }

                this.logger.Debug("The weighting scale service has started.");
                this.isStarted = false;

                //await this.ClearMessageAsync();

                await this.StartWeightAcquisitionAsync();
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Error while initializing the weighting scale service.");
                this.isStarted = false;
                throw;
            }
        }

        public async Task StartWeightAcquisitionAsync()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WeightingScaleService));
            }

            if (!this.isDeviceEnabled)
            {
                throw new InvalidOperationException(
                   "Cannot perform the operation because the weighting scale service is not enabled.");
            }

            this.logger.Debug("Starting continuous weight acquisition ...");
            if (this.UnitaryWeight >= 0)
            {
                await this.deviceDriver.SetAverageUnitaryWeightAsync(this.UnitaryWeight);
            }

            this.weightPollTimer.Change(0, WeightPollInterval);
        }

        public async Task StopAsync()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WeightingScaleService));
            }

            //if (!this.isStarted)
            //{
            //    this.logger.Debug("The weighting scale service is already stopped.");
            //    return Task.CompletedTask;
            //}

            this.logger.Info("Stopping the weighting scale service ...");
            if (this.UnitaryWeight >= 0)
            {
                //this.deviceDriver.SetAverageUnitaryWeightAsync(0.0F);
                await this.ClearMessageAsync();
            }

            this.isStarted = false;
            this.weightPollTimer.Change(Timeout.Infinite, Timeout.Infinite);

            this.deviceDriver.DisconnectAsync();

            this.logger.Debug("The weighting scale service has stopped.");
        }

        public void StopWeightAcquisitionAsync()
        {
            this.logger.Debug("Stopping continuous weight acquisition ...");
            this.weightPollTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public async Task UpdateItemAverageWeightAsync(int itemId, double? averageWeight)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WeightingScaleService));
            }

            if (!this.isDeviceEnabled)
            {
                throw new InvalidOperationException(
                    "Cannot perform the operation because the weighting scale service is not enabled.");
            }

            await this.machineItemsWebService.UpdateAverageWeightAsync(itemId, averageWeight ?? 0);
        }

        public async Task UpdateSettingsAsync(bool isEnabled, string ipAddress, int port, WeightingScaleModelNumber modelNumber)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WeightingScaleService));
            }

            this.logger.Debug("Updating the weighting scale settings on MAS ...");

            await this.accessoriesWebService.UpdateWeightingScaleSettingsAsync(isEnabled, ipAddress, port, modelNumber);

            this.logger.Debug("The weighting scale settings were updated.");

            this.isDeviceEnabled = isEnabled;
            if (this.isDeviceEnabled)
            {
                await this.StopAsync();
                await this.StartAsync();
                await this.MeasureWeightAsync(false);
            }
            else
            {
                await this.StopAsync();
            }
        }

        private async Task OnWeightPollTimerTickAsync()
        {
            if (this.isDisposed || !this.isDeviceEnabled)
            {
                return;
            }

            if (this.WeighAcquired is null)
            {
                this.logger.Debug("There are no subscribers to the WeightAcquired event.");

                this.StopWeightAcquisitionAsync();
            }
            else
            {
                try
                {
                    this.weightPollTimer.Change(Timeout.Infinite, Timeout.Infinite);
                    var weightSample = await this.deviceDriver.MeasureWeightAsync(true);
                    if (weightSample is null)
                    {
                        this.ShowNotification(Resources.Localized.Get("OperatorApp.ScaleNotResponding"), NotificationSeverity.Error);
                    }
                    else
                    {
                        this.logger.Debug($"Weighting scale #{weightSample.ScaleNumber} detected a weight of {weightSample.Weight}{weightSample.UnitOfMeasure} ({weightSample.Quality}); count {weightSample.UnitsCount}.");

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            this.WeighAcquired?.Invoke(this, new WeightAcquiredEventArgs(weightSample))
                        );
                    }
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, "Error while performing continuous weight sampling.");
                }
                this.weightPollTimer.Change(WeightPollInterval, WeightPollInterval);
            }
        }

        private void ShowNotification(string message, NotificationSeverity severity = NotificationSeverity.Info)
        {
            if (this.deviceDriver.ShowScaleNotResponding)
            {
                this.eventAggregator
                  .GetEvent<PresentationNotificationPubSubEvent>()
                  .Publish(new PresentationNotificationMessage(message, severity));
            }
        }

        #endregion
    }
}
