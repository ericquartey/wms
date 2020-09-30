using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
using Microsoft.Extensions.Logging;
using Realms;

namespace Ferretto.VW.TelemetryService.Providers
{
    internal sealed class ScreenShotProvider : IScreenShotProvider
    {
        #region Fields

        private readonly ILogger<ErrorLogProvider> logger;

        private readonly Realm realm;

        #endregion

        #region Constructors

        public ScreenShotProvider(Realm realm,
                                  ILogger<ErrorLogProvider> logger)
        {
            this.realm = realm;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public void DeleteOldLogs(TimeSpan maximumLogTimespan)
        {
            this.logger.LogDebug("Deleting old screenShot ...");

            using var trans = this.realm.BeginWrite();

            var screenShotLogs = this.realm.All<Models.ScreenShot>();

            var lastLog = screenShotLogs.OrderByDescending(e => e.TimeStamp).FirstOrDefault();

            if (lastLog is null)
            {
                return;
            }

            var minTimestamp = lastLog.TimeStamp - maximumLogTimespan;

            var logsToDelete = screenShotLogs.Where(e => e.TimeStamp < minTimestamp);

            var countLogsToDelete = logsToDelete.Count();

            this.realm.RemoveRange<Models.ScreenShot>(logsToDelete);

            trans.Commit();

            this.logger.LogDebug($"A total of {countLogsToDelete} screenShot were deleted.");
        }

        public IEnumerable<IScreenShot> GetAll()
        {
            return this.realm.All<Models.ScreenShot>().ToArray();
        }

        public async Task SaveAsync(string serialNumber, IScreenShot screenShot)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new System.ArgumentException("The machine's serial number cannot be empty.", nameof(serialNumber));
            }

            if (screenShot is null)
            {
                throw new System.ArgumentNullException(nameof(screenShot));
            }

            var machine = this.realm.All<Models.Machine>().SingleOrDefault(m => m.SerialNumber == serialNumber);
            if (machine is null)
            {
                throw new System.ArgumentException($"No machine corresponding to the serial '{serialNumber}' was found.");
            }

            var newId = 0;
            if (this.realm.All<Models.ScreenShot>().OrderByDescending(e => e.Id).FirstOrDefault() is Models.ScreenShot screenShotFound)
            {
                newId = screenShotFound.Id + 1;
            }

            var screenShotEntry = new Models.ScreenShot
            {
                Id = newId,
                BayNumber = screenShot.BayNumber,
                Machine = machine,
                Image = screenShot.Image,
                TimeStamp = screenShot.TimeStamp,
                ViewName = screenShot.ViewName,
            };

            await this.realm.WriteAsync(r => r.Add(screenShotEntry));
        }

        #endregion
    }
}
