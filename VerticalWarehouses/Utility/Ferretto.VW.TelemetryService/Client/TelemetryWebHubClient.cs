using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry.Hubs;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.Common.Hubs;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Ferretto.VW.TelemetryService
{
    public class TelemetryWebHubClient : AutoReconnectHubClient, ITelemetryWebHubClient
    {
        #region Fields

        private readonly IServiceScopeFactory serviceScopeFactory;

        #endregion

        #region Constructors

        public TelemetryWebHubClient(Uri uri, IServiceScopeFactory serviceScopeFactory)
            : base(uri)
        {
            this.serviceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Methods

        public async Task SendErrorLogAsync(string serialNumber, ErrorLog errorLog)
        {
            await this.TrySendErrorLogAsync(serialNumber, errorLog, persistOnSendFailure: true);
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

        public async Task SendScreenCastAsync(int bayNumber, string serialNumber, DateTimeOffset timeStamp, byte[] screenshot)
        {
            if (this.IsConnected)
            {
                await this.SendAsync("SendScreenCast", bayNumber, serialNumber, timeStamp, screenshot);
            }
        }

        public async Task SendScreenShotAsync(int bayNumber, string serialNumber, DateTimeOffset timeStamp, byte[] screenshot)
        {
            if (this.IsConnected)
            {
                await this.SendAsync("SendScreenShot", bayNumber, serialNumber, timeStamp, screenshot);
            }
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

            var realm = scope.ServiceProvider.GetRequiredService<Realms.Realm>();
            foreach (var missionLog in realm.All<Models.MissionLog>().ToArray())
            {
                var success = await this.TrySendMissionLogAsync(machine.SerialNumber, missionLog, persistOnSendFailure: false);
                if (success)
                {
                    await realm.WriteAsync(r =>
                    {
                        r.Remove(missionLog);
                    });
                }
            }

            foreach (var errorLog in realm.All<Models.ErrorLog>().ToArray())
            {
                var success = await this.TrySendErrorLogAsync(machine.SerialNumber, errorLog, persistOnSendFailure: false);
                if (success)
                {
                    await realm.WriteAsync(r =>
                    {
                        r.Remove(errorLog);
                    });
                }
            }
        }

        protected override void RegisterEvents(HubConnection connection)
        {
            // do nothing
            // no incoming notifications from the hub
        }

        private async Task SaveEntryAsync(string serialNumber, IErrorLog errorLog)
        {
            var scope = this.serviceScopeFactory.CreateScope();

            var errorLogProvider = scope.ServiceProvider.GetRequiredService<Providers.IErrorLogProvider>();
            await errorLogProvider.SaveAsync(serialNumber, errorLog);
        }

        private async Task SaveEntryAsync(string serialNumber, IMissionLog missionLog)
        {
            var scope = this.serviceScopeFactory.CreateScope();
            // TODO: move logic to provider
            var realm = scope.ServiceProvider.GetRequiredService<Realms.Realm>();
            var machine = realm.All<Models.Machine>().SingleOrDefault(m => m.SerialNumber == serialNumber);

            if (machine != null)
            {
                var logEntry = new Models.MissionLog
                {
                    Bay = missionLog.Bay,
                    CellId = missionLog.CellId,
                    Direction = missionLog.Direction,
                    EjectLoadUnit = missionLog.EjectLoadUnit,
                    LoadUnitId = missionLog.LoadUnitId,
                    Machine = machine,
                    MissionId = missionLog.MissionId,
                    MissionType = missionLog.MissionType,
                    Priority = missionLog.Priority,
                    Status = missionLog.Status,
                    Stage = missionLog.Stage,
                    StopReason = missionLog.StopReason,
                    TimeStamp = missionLog.TimeStamp,
                    WmsId = missionLog.WmsId,
                };

                await realm.WriteAsync(r => r.Add(logEntry));
            }
        }

        private async Task<bool> TrySendErrorLogAsync(string serialNumber, IErrorLog errorLog, bool persistOnSendFailure)
        {
            var messageSent = false;
            if (this.IsConnected)
            {
                try
                {
                    await this.SendAsync(nameof(ITelemetryHub.SendErrorLog), serialNumber, errorLog);

                    messageSent = true;
                }
                catch (Exception ex)
                {
                }
            }

            if (!messageSent && persistOnSendFailure)
            {
                await this.SaveEntryAsync(serialNumber, errorLog);
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
                catch (Exception ex)
                {
                }
            }

            if (!messageSent && persistOnSendFailure)
            {
                await this.SaveEntryAsync(serialNumber, missionLog);
            }

            return messageSent;
        }

        #endregion
    }
}
