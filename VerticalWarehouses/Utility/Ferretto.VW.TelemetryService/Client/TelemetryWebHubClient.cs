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
            await this.TrySendScreenShotAsync(serialNumber, bayNumber, timeStamp, screenshot, persistOnSendFailure: true);
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

            foreach (var screenShot in realm.All<Models.ScreenShot>().ToArray())
            {
                if (screenShot.Image != null)
                {
                    var success = await this.TrySendScreenShotAsync(machine.SerialNumber, screenShot.BayNumber, screenShot.TimeStamp, screenShot.Image, persistOnSendFailure: false);
                    if (success)
                    {
                        await realm.WriteAsync(r =>
                        {
                            r.Remove(screenShot);
                        });
                    }
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

            var missionLogProvider = scope.ServiceProvider.GetRequiredService<Providers.IMissionLogProvider>();
            await missionLogProvider.SaveAsync(serialNumber, missionLog);
        }

        private async Task SaveEntryAsync(string serialNumber, IScreenShot screenShot)
        {
            var scope = this.serviceScopeFactory.CreateScope();

            var screenShotProvider = scope.ServiceProvider.GetRequiredService<Providers.IScreenShotProvider>();
            await screenShotProvider.SaveAsync(serialNumber, screenShot);
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
                catch
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

        #endregion
    }
}
