using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ferretto.VW.App.Accessories.Interfaces.WeightingScale;
using Ferretto.VW.Devices.WeightingScale;
using Ferretto.VW.MAS.AutomationService.Contracts;
using NLog;

namespace Ferretto.VW.App.Accessories
{
    internal sealed partial class WeightingScaleService : Interfaces.IWeightingScaleService
    {
        #region Fields

        private const int WeightPollInterval = 800;

        private readonly IMachineAccessoriesWebService accessoriesWebService;

        private readonly IWeightingScaleDriver deviceDriver;

        private readonly Devices.DeviceInformation deviceInformation = new Devices.DeviceInformation();

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly IMachineItemsWebService machineItemsWebService;

        private readonly Timer weightPollTimer;

        private bool isDeviceEnabled;

        private bool isStarted;

        #endregion

        #region Constructors

        public WeightingScaleService(
            IWeightingScaleDriver deviceDriver,
            IMachineAccessoriesWebService accessoriesWebService,
            IMachineItemsWebService machineItemsWebService)
        {
            this.deviceDriver = deviceDriver;
            this.accessoriesWebService = accessoriesWebService;
            this.machineItemsWebService = machineItemsWebService;

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

        public async Task<IWeightSample> MeasureWeightAsync()
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

            return await this.deviceDriver.MeasureWeightAsync();
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

                await this.deviceDriver.ConnectAsync(accessories.WeightingScale.IpAddress, accessories.WeightingScale.TcpPort);

                this.deviceInformation.FirmwareVersion = await this.deviceDriver.RetrieveVersionAsync();

                if (!this.DeviceInformation.IsEmpty)
                {
                    await this.accessoriesWebService.UpdateWeightingScaleDeviceInfoAsync(
                        new DeviceInformation
                        {
                            FirmwareVersion = this.DeviceInformation.FirmwareVersion,
                            ManufactureDate = this.DeviceInformation.ManufactureDate,
                            ModelNumber = this.DeviceInformation.ModelNumber,
                            SerialNumber = this.DeviceInformation.SerialNumber
                        });
                }

                this.logger.Debug("The weighting scale service has started.");
                this.isStarted = false;
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

        public Task StopAsync()
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

            this.isStarted = false;
            this.weightPollTimer.Change(Timeout.Infinite, Timeout.Infinite);

            this.deviceDriver.Disconnect();

            this.logger.Debug("The weighting scale service has stopped.");

            return Task.CompletedTask;
        }

        public void StopWeightAcquisition()
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

        public async Task UpdateSettingsAsync(bool isEnabled, string ipAddress, int port)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(WeightingScaleService));
            }

            this.logger.Debug("Updating the weighting scale settings on MAS ...");

            await this.accessoriesWebService.UpdateWeightingScaleSettingsAsync(isEnabled, ipAddress, port);

            this.logger.Debug("The weighting scale settings were updated.");

            this.isDeviceEnabled = isEnabled;
            if (this.isDeviceEnabled)
            {
                await this.StartAsync();
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

                this.StopWeightAcquisition();
            }
            else
            {
                try
                {
                    var weightSample = await this.deviceDriver.MeasureWeightAsync();
                    if (weightSample is null)
                    {
                        this.logger.Warn("No sample was received from the scale.");
                        return;
                    }

                    this.logger.Debug($"Weighting scale #{weightSample.ScaleNumber} detected a weight of {weightSample.Weight}{weightSample.UnitOfMeasure} ({weightSample.Quality}).");

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        this.WeighAcquired?.Invoke(this, new WeightAcquiredEventArgs(weightSample))
                    );
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex, "Error while performing continuous weight sampling.");
                }
            }
        }

        #endregion
    }
}
