using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.ServiceDesk.Telemetry.Hubs;
using Ferretto.VW.Common.Hubs;
using Ferretto.VW.TelemetryService.Providers;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.TelemetryService
{
    public class TelemetryWebHubClient : AutoReconnectHubClient, ITelemetryWebHubClient
    {
        #region Fields

        public const int IO_ERROR_INTERVAL_SECONDS = -10;

        private readonly NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Uri logUri;

        private readonly IServiceScopeFactory serviceScopeFactory;

        #endregion

        #region Constructors

        public TelemetryWebHubClient(Uri uri, IServiceScopeFactory serviceScopeFactory)
            : base(uri)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logUri = uri;
            this.logger.Info($"Host url {this.logUri}");
            this.ConnectionStatusChanged += async (s, e) => await this.OnConnectionStatusChanged(e);
        }

        public TelemetryWebHubClient(Uri uri, IServiceScopeFactory serviceScopeFactory, WebProxy webProxy)
                    : base(uri, webProxy)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.logUri = uri;
            this.logger.Info($"Host url {this.logUri} WebProxy {webProxy.Address}");
            this.ConnectionStatusChanged += async (s, e) => await this.OnConnectionStatusChanged(e);
        }

        #endregion

        #region Methods

        public async Task PersistIOLogAsync(string serialNumber, IOLog ioLog)
        {
            this.TryPersistIOLogAsync(serialNumber, ioLog);
        }

        public async Task SendErrorLogAsync(string serialNumber, ErrorLog errorLog)
        {
            await this.TrySendErrorLogAsync(serialNumber, errorLog, persistOnSendFailure: true);
        }

        public async Task SendIOLogAsync(string serialNumber, IOLog ioLog)
        {
            await this.TrySendIOLogAsync(serialNumber, ioLog, persistOnSendFailure: true);
        }

        public async Task SendMachineAsync(Machine machine)
        {
            if (this.IsConnected)
            {
                await this.SendAsync("SendMachine", machine);
            }
        }

        public async Task SendMissionLogAsync(string serialNumber, MissionLog missionLog)
        {
            await this.TrySendMissionLogAsync(serialNumber, missionLog, persistOnSendFailure: true);
        }

        public async Task SendRawDatabaseContentAsync(string serialNumber, byte[] rawDatabaseContent)
        {
            await this.TrySendRawDatabaseContentAsync(serialNumber, rawDatabaseContent, persistOnSendFailure: true);
        }

        public async Task SendScreenCastAsync(int bayNumber, string serialNumber, DateTimeOffset timeStamp, byte[] screenshot)
        {
            if (this.IsConnected)
            {
                await this.SendAsync("SendScreenCast", bayNumber, serialNumber, timeStamp, screenshot);
            }
        }

        public async Task SendScreenShotAsync(int bayNumber, string serialNumber, DateTimeOffset timeStamp, byte[] screenshot)
        {
            await this.TrySendScreenShotAsync(serialNumber, bayNumber, timeStamp, screenshot, persistOnSendFailure: true);
        }

        public async Task SendServicingInfoAsync(string serialNumber, ServicingInfo servicingInfo)
        {
            await this.TrySendServicingInfoAsync(serialNumber, servicingInfo, persistOnSendFailure: true);
        }

        protected override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            var scope = this.serviceScopeFactory.CreateScope();
            var machineProvider = scope.ServiceProvider.GetRequiredService<Providers.IMachineProvider>();

            var machine = machineProvider.Get();
            if (machine is null)
            {
                // TODO: print warning
                return;
            }

            await this.SendAsync(nameof(ITelemetryHub.SendMachine), machine);

            await this.SendSavedMissionLog(machine, scope);

            await this.SendSavedErrorLog(machine, scope);

            await this.SendSavedServicingInfo(machine, scope);

            await this.SendSavedScreenshot(machine, scope);

            await this.SendSavedRawDatabase(scope);
        }

        protected override void RegisterEvents(HubConnection connection)
        {
            // do nothing
            // no incoming notifications from the hub
        }

        private IEnumerable<IIOLog> GetEntryAsync(string serialNumber, DateTimeOffset start, DateTimeOffset end)
        {
            var scope = this.serviceScopeFactory.CreateScope();

            var ioLogProvider = scope.ServiceProvider.GetRequiredService<Providers.IIOLogProvider>();
            return ioLogProvider.GetByTimeStamp(serialNumber, start, end);
        }

        private Task OnConnectionStatusChanged(ConnectionStatusChangedEventArgs e)
        {
            this.logger.Info($"Connection {e.IsConnected} to {this.logUri}");
            return Task.CompletedTask;
        }

        private void SaveEntry(byte[] rawDatabaseContent)
        {
            var scope = this.serviceScopeFactory.CreateScope();

            var machineProvider = scope.ServiceProvider.GetRequiredService<Providers.IMachineProvider>();
            machineProvider.SaveRawDatabaseContent(rawDatabaseContent);
        }

        private void SaveEntryAsync(string serialNumber, IErrorLog errorLog)
        {
            var scope = this.serviceScopeFactory.CreateScope();

            var errorLogProvider = scope.ServiceProvider.GetRequiredService<Providers.IErrorLogProvider>();
            errorLogProvider.SaveAsync(serialNumber, errorLog);
        }

        private void SaveEntryAsync(string serialNumber, IIOLog ioLog)
        {
            var scope = this.serviceScopeFactory.CreateScope();

            var ioLogProvider = scope.ServiceProvider.GetRequiredService<Providers.IIOLogProvider>();
            ioLogProvider.SaveAsync(serialNumber, ioLog);
        }

        private async Task SaveEntryAsync(string serialNumber, IMissionLog missionLog)
        {
            var scope = this.serviceScopeFactory.CreateScope();

            var missionLogProvider = scope.ServiceProvider.GetRequiredService<Providers.IMissionLogProvider>();
            missionLogProvider.SaveAsync(serialNumber, missionLog);
        }

        private async Task SaveEntryAsync(string serialNumber, IServicingInfo servicingInfo)
        {
            var scope = this.serviceScopeFactory.CreateScope();

            var servicingInfoProvider = scope.ServiceProvider.GetRequiredService<Providers.IServicingInfoProvider>();
            servicingInfoProvider.SaveAsync(serialNumber, servicingInfo);
        }

        private async Task SaveEntryAsync(string serialNumber, IScreenShot screenShot)
        {
            var scope = this.serviceScopeFactory.CreateScope();

            var screenShotProvider = scope.ServiceProvider.GetRequiredService<Providers.IScreenShotProvider>();
            screenShotProvider.SaveAsync(serialNumber, screenShot);
        }

        private async Task<bool> SendSavedErrorLog(IMachine machine, IServiceScope scope)
        {
            try
            {
                this.logger.Debug("Send saved ErrorLog");
                var success = false;

                var errorProvider = scope.ServiceProvider.GetRequiredService<IErrorLogProvider>();
                var errors = errorProvider.GetAllId();
                foreach (var errorLog in errors)
                {
                    success = await this.TrySendErrorLogAsync(machine.SerialNumber, errorLog, persistOnSendFailure: false);
                    if (success)
                    {
                        success = await this.SendSavedIoLog(machine, scope, errorLog.OccurrenceDate);
                    }
                    if (!success)
                    {
                        break;
                    }
                }

                if (success)
                {
                    errorProvider.Remove(errors);
                }
                return success;
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
                return false;
            }
        }

        private async Task<bool> SendSavedIoLog(IMachine machine, IServiceScope scope, DateTimeOffset date)
        {
            var successIo = false;
            try
            {
                var ioProvider = scope.ServiceProvider.GetRequiredService<IIOLogProvider>();
                var ioLogs = ioProvider.GetAllId();
                var ioLogsToBeRemoved = ioLogs.Where(io => io.TimeStamp >= date.AddSeconds(IO_ERROR_INTERVAL_SECONDS) && io.TimeStamp <= date.AddSeconds(1));

                if (ioLogsToBeRemoved.Any())
                {
                    foreach (var ioLog in ioLogsToBeRemoved)
                    {
                        successIo = await this.TrySendIOLogAsync(machine.SerialNumber, ioLog, false);
                        if (!successIo)
                        {
                            break;
                        }
                    }
                    if (successIo)
                    {
                        ioProvider.Remove(ioLogsToBeRemoved);
                    }
                }
                else
                {
                    successIo = true;
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
                successIo = false;
            }

            return successIo;
        }

        private async Task SendSavedMissionLog(IMachine machine, IServiceScope scope)
        {
            try
            {
                this.logger.Debug("Send saved MissionLog");
                var missionProvider = scope.ServiceProvider.GetRequiredService<IMissionLogProvider>();
                var missions = missionProvider.GetAllId();
                var success = false;
                foreach (var missionLog in missions)
                {
                    success = await this.TrySendMissionLogAsync(machine.SerialNumber, missionLog, persistOnSendFailure: false);
                    if (!success)
                    {
                        break;
                    }
                }
                if (success)
                {
                    missionProvider.Remove(missions);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
        }

        private async Task SendSavedRawDatabase(IServiceScope scope)
        {
            try
            {
                var machineProvider = scope.ServiceProvider.GetRequiredService<IMachineProvider>();
                var machine = machineProvider.GetRaw();
                if (machine != null && machine.RawDatabaseContent != null)
                {
                    this.logger.Debug("Send saved RawDatabase");
                    var success = await this.TrySendRawDatabaseContentAsync(machine.SerialNumber, machine.RawDatabaseContent, persistOnSendFailure: false);
                    if (success)
                    {
                        machine.RawDatabaseContent = null;
                        machineProvider.SaveRawDatabaseContent(null);
                    }
                    else
                    {
                        this.logger.Error("RawDatabase not sent");
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
        }

        private async Task SendSavedScreenshot(IMachine machine, IServiceScope scope)
        {
            try
            {
                this.logger.Debug("Send saved ScreenShot");
                var success = false;
                var screenShotProvider = scope.ServiceProvider.GetRequiredService<IScreenShotProvider>();
                var screenShots = screenShotProvider.GetAllId();
                foreach (var screenShot in screenShots)
                {
                    if (screenShot.Image != null)
                    {
                        success = await this.TrySendScreenShotAsync(machine.SerialNumber, screenShot.BayNumber, screenShot.TimeStamp, screenShot.Image, persistOnSendFailure: false);
                        if (!success)
                        {
                            break;
                        }
                    }
                }
                if (success)
                {
                    screenShotProvider.Remove(screenShots);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
        }

        private async Task SendSavedServicingInfo(IMachine machine, IServiceScope scope)
        {
            try
            {
                this.logger.Debug("Send saved ServicingInfo");
                var success = false;
                var servicingInfoProvider = scope.ServiceProvider.GetRequiredService<IServicingInfoProvider>();
                var sInfos = servicingInfoProvider.GetAllId();
                foreach (var sInfo in sInfos)
                {
                    success = await this.TrySendServicingInfoAsync(machine.SerialNumber, sInfo, persistOnSendFailure: false);
                    if (!success)
                    {
                        break;
                    }
                }
                if (success)
                {
                    servicingInfoProvider.Remove(sInfos);
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex);
            }
        }

        private bool TryPersistIOLogAsync(string serialNumber, IIOLog ioLog)
        {
            var messagePersited = false;

            try
            {
                this.SaveEntryAsync(serialNumber, ioLog);
                messagePersited = true;
            }
            catch
            {
            }

            return messagePersited;
        }

        private async Task<bool> TrySendErrorLogAsync(string serialNumber, IErrorLog errorLog, bool persistOnSendFailure)
        {
            var messageSent = false;
            if (this.IsConnected)
            {
                try
                {
                    await this.SendAsync(nameof(ITelemetryHub.SendErrorLog), serialNumber, errorLog);

                    if (persistOnSendFailure)
                    {
                        foreach (var ioLog in this.GetEntryAsync(serialNumber, errorLog.OccurrenceDate.AddSeconds(IO_ERROR_INTERVAL_SECONDS), errorLog.OccurrenceDate.AddSeconds(1)))
                        {
                            var ioLogCorrect = new IOLog
                            {
                                BayNumber = ioLog.BayNumber,
                                Description = ioLog.Description,
                                Input = ioLog.Input,
                                Output = ioLog.Output,
                                TimeStamp = ioLog.TimeStamp.ToOffset(DateTimeOffset.Now.Offset)
                            };

                            await this.SendAsync(nameof(ITelemetryHub.SendIOLog), serialNumber, ioLogCorrect);
                        }
                    }

                    messageSent = true;
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex);
                }
            }

            if (!messageSent && persistOnSendFailure)
            {
                this.SaveEntryAsync(serialNumber, errorLog);
            }

            return messageSent;
        }

        private async Task<bool> TrySendIOLogAsync(string serialNumber, IIOLog ioLog, bool persistOnSendFailure)
        {
            var messageSent = false;
            if (this.IsConnected)
            {
                try
                {
                    await this.SendAsync(nameof(ITelemetryHub.SendIOLog), serialNumber, ioLog);

                    messageSent = true;
                }
                catch (Exception ex)
                {
                    this.logger.Error(ex);
                }
            }

            if (!messageSent && persistOnSendFailure)
            {
                this.SaveEntryAsync(serialNumber, ioLog);
            }

            return messageSent;
        }

        private async Task<bool> TrySendMissionLogAsync(string serialNumber, IMissionLog missionLog, bool persistOnSendFailure)
        {
            var messageSent = false;

            if (this.IsConnected)
            {
                try
                {
                    await this.SendAsync(nameof(ITelemetryHub.SendMissionLog), serialNumber, missionLog);

                    messageSent = true;
                }
                catch
                {
                }
            }

            if (!messageSent && persistOnSendFailure)
            {
                await this.SaveEntryAsync(serialNumber, missionLog);
            }

            return messageSent;
        }

        private async Task<bool> TrySendRawDatabaseContentAsync(string serialNumber, byte[] rawDatabaseContent, bool persistOnSendFailure)
        {
            var messageSent = false;

            if (this.IsConnected)
            {
                try
                {
                    await this.SendAsync(nameof(ITelemetryHub.SendRawDatabaseContent), serialNumber, rawDatabaseContent);

                    messageSent = true;
                }
                catch
                {
                }
            }

            if (!messageSent && persistOnSendFailure)
            {
                this.SaveEntry(rawDatabaseContent);
            }

            return messageSent;
        }

        private async Task<bool> TrySendScreenShotAsync(string serialNumber, int bayNumber, DateTimeOffset timeStamp, byte[] image, bool persistOnSendFailure)
        {
            var messageSent = false;

            if (this.IsConnected)
            {
                try
                {
                    await this.SendAsync(nameof(ITelemetryHub.SendScreenShot), bayNumber, serialNumber, timeStamp, image);

                    messageSent = true;
                }
                catch
                {
                }
            }

            if (!messageSent && persistOnSendFailure)
            {
                var screenShot = new ScreenShot
                {
                    BayNumber = bayNumber,
                    TimeStamp = timeStamp,
                    Image = image,
                };

                await this.SaveEntryAsync(serialNumber, screenShot);
            }

            return messageSent;
        }

        private async Task<bool> TrySendServicingInfoAsync(string serialNumber, IServicingInfo servicingInfo, bool persistOnSendFailure)
        {
            var messageSent = false;

            if (this.IsConnected)
            {
                try
                {
                    await this.SendAsync(nameof(ITelemetryHub.SendServicingInfo), serialNumber, servicingInfo);

                    messageSent = true;
                }
                catch
                {
                }
            }

            if (!messageSent && persistOnSendFailure)
            {
                await this.SaveEntryAsync(serialNumber, servicingInfo);
            }

            return messageSent;
        }

        #endregion
    }
}
