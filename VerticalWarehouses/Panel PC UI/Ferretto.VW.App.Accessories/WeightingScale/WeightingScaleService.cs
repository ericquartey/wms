using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Services;
using Ferretto.VW.Devices.WeightingScale;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Accessories
{
    internal sealed partial class WeightingScaleService : Interfaces.IWeightingScaleService
    {
        #region Fields

        private readonly IMachineAccessoriesWebService accessoriesWebService;

        private readonly IWeightingScaleDriver deviceDriver;

        private readonly IEventAggregator eventAggregator;

        private bool isDeviceEnabled;

        #endregion

        #region Constructors

        public WeightingScaleService(
            IWeightingScaleDriver deviceDriver,
            IMachineAccessoriesWebService accessoriesWebService,
            IEventAggregator eventAggregator)
        {
            this.deviceDriver = deviceDriver;
            this.accessoriesWebService = accessoriesWebService;
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public Devices.DeviceInformation DeviceInformation => throw new NotImplementedException();

        #endregion

        #region Methods

        public async Task ClearMessageAsync()
        {
            await this.deviceDriver.ClearMessageAsync();
        }

        public async Task DisplayMessageAsync(string message)
        {
            await this.deviceDriver.DisplayMessageAsync(message);
        }

        public async Task DisplayMessageAsync(string message, TimeSpan duration)
        {
            await this.deviceDriver.DisplayMessageAsync(message, duration);
        }

        public async Task<IWeightSample> MeasureWeightAsync()
        {
            return await this.deviceDriver.MeasureWeightAsync();
        }

        public async Task ResetAverageUnitaryWeightAsync()
        {
            await this.deviceDriver.ResetAverageUnitaryWeightAsync();
        }

        public async Task SetAverageUnitaryWeightAsync(float weight)
        {
            await this.deviceDriver.SetAverageUnitaryWeightAsync(weight);
        }

        public async Task StartAsync()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(BarcodeReaderService));
            }

            try
            {
                this.InitializeSerialPortsTimer();

                /*    var accessories = await this.accessoriesWebService.GetAllAsync();
                    this.isDeviceEnabled = accessories.BarcodeReader?.IsEnabledNew == true;
                    if (this.isDeviceEnabled)
                    {
                        this.deviceDriver.Connect(
                            new NewlandSerialPortOptions
                            {
                                PortName = accessories.BarcodeReader.PortName,
                                DeviceModel = this.DeviceModel
                            });
                        this.isStarted = true;

                        if (!this.DeviceInformation.IsEmpty)
                        {
                            await this.accessoriesWebService.UpdateBarcodeReaderDeviceInfoAsync(
                                new MAS.AutomationService.Contracts.DeviceInformation
                                {
                                    FirmwareVersion = this.DeviceInformation.FirmwareVersion,
                                    ManufactureDate = this.DeviceInformation.ManufactureDate,
                                    ModelNumber = this.DeviceInformation.ModelNumber,
                                    SerialNumber = this.DeviceInformation.SerialNumber
                                });
                        }
                    }*/
            }
            catch (Exception ex)
            {
                this.NotifyError($"Impossibile comunicare con la bilancia. {ex.Message}");
            }
        }

        public Task StopAsync()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(nameof(BarcodeReaderService));
            }

            try
            {
                this.deviceDriver.Disconnect();
                this.DisableSerialPortsTimer();
            }
            catch (Exception ex)
            {
                this.NotifyError(ex);
            }

            return Task.CompletedTask;
        }

        public Task UpdateItemAverageWeightAsync(int itemId, double averageWeight)
        {
            //TODO
            return Task.CompletedTask;
        }

        public async Task UpdateSettingsAsync(bool isEnabled, string portName)
        {
            try
            {
                await this.accessoriesWebService.UpdateWeightingScaleSettingsAsync(isEnabled, portName);

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
            catch (Exception ex)
            {
                this.NotifyError(ex);
            }
        }

        private void NotifyError(Exception ex)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(ex));
        }

        private void NotifyError(string message)
        {
            this.eventAggregator
                .GetEvent<PresentationNotificationPubSubEvent>()
                .Publish(new PresentationNotificationMessage(message, Services.Models.NotificationSeverity.Error));
        }

        #endregion
    }
}
