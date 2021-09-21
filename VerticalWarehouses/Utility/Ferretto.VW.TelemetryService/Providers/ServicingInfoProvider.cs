using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
using Microsoft.Extensions.Logging;
using Realms;

namespace Ferretto.VW.TelemetryService.Providers
{
    internal sealed class ServicingInfoProvider : IServicingInfoProvider
    {
        #region Fields

        private readonly ILogger<ServicingInfoProvider> logger;

        private readonly Realm realm;

        #endregion

        #region Constructors

        public ServicingInfoProvider(Realm realm, ILogger<ServicingInfoProvider> logger)
        {
            this.realm = realm;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public void DeleteOldLogs(TimeSpan maximumLogTimespan)
        {
            this.logger.LogDebug("Deleting old servicing info ...");

            using var trans = this.realm.BeginWrite();

            var servicingInfo = this.realm.All<Models.ServicingInfo>();

            var lastLog = servicingInfo.OrderByDescending(e => e.TimeStamp).FirstOrDefault();

            if (lastLog is null)
            {
                return;
            }

            var minTimestamp = lastLog.TimeStamp - maximumLogTimespan;

            var logsToDelete = servicingInfo.Where(e => e.TimeStamp < minTimestamp);

            var countLogsToDelete = logsToDelete.Count();

            this.realm.RemoveRange(logsToDelete);

            trans.Commit();

            this.logger.LogDebug($"A total of {countLogsToDelete} servicing info were deleted.");
        }

        public IEnumerable<IServicingInfo> GetAll()
        {
            return this.realm.All<Models.ServicingInfo>().ToArray();
        }

        public async Task SaveAsync(string serialNumber, IServicingInfo servicingInfo)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new System.ArgumentException("The machine's serial number cannot be empty.", nameof(serialNumber));
            }

            if (servicingInfo is null)
            {
                throw new System.ArgumentNullException(nameof(servicingInfo));
            }

            var machine = this.realm.All<Models.Machine>().SingleOrDefault(m => m.SerialNumber == serialNumber);
            if (machine is null)
            {
                throw new System.ArgumentException($"No machine corresponding to the serial '{serialNumber}' was found.");
            }

            var newId = 0;
            if (this.realm.All<Models.ServicingInfo>().OrderByDescending(e => e.Id).FirstOrDefault() is Models.ServicingInfo service)
            {
                newId = service.Id + 1;
            }

            var servicingInfoEntity = new Models.ServicingInfo
            {
                Id = newId,
                InstallationDate = servicingInfo.InstallationDate,
                IsHandOver = servicingInfo.IsHandOver,
                LastServiceDate = servicingInfo.LastServiceDate,
                NextServiceDate = servicingInfo.NextServiceDate,
                ServiceStatusId = servicingInfo.ServiceStatusId,
                TimeStamp = servicingInfo.TimeStamp,
                TotalMissions = servicingInfo.TotalMissions
            };

            await this.realm.WriteAsync(r => r.Add(servicingInfoEntity));
        }

        #endregion
    }
}
