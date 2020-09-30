using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Ferretto.ServiceDesk.Telemetry;
using Microsoft.Extensions.Logging;
using Realms;

namespace Ferretto.VW.TelemetryService.Providers
{
    internal sealed class MissionLogProvider : IMissionLogProvider
    {
        #region Fields

        private readonly ILogger<MissionLogProvider> logger;

        private readonly Realm realm;

        #endregion

        #region Constructors

        public MissionLogProvider(Realm realm,
                                  ILogger<MissionLogProvider> logger)
        {
            this.realm = realm;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public void DeleteOldLogs(TimeSpan maximumLogTimespan)
        {
            this.logger.LogDebug("Deleting old mission logs ...");

            using var trans = this.realm.BeginWrite();

            var missionLogs = this.realm.All<Models.MissionLog>();

            var lastLog = missionLogs.OrderByDescending(e => e.TimeStamp).FirstOrDefault();

            if (lastLog is null)
            {
                return;
            }

            var minTimestamp = lastLog.TimeStamp - maximumLogTimespan;

            var logsToDelete = missionLogs.Where(e => e.TimeStamp < minTimestamp);

            var countLogsToDelete = logsToDelete.Count();

            this.realm.RemoveRange(logsToDelete);

            trans.Commit();

            this.logger.LogDebug($"A total of {countLogsToDelete} mission logs were deleted.");
        }

        public IEnumerable<IMissionLog> GetAll()
        {
            return this.realm.All<Models.MissionLog>().ToArray();
        }

        public async Task SaveAsync(string serialNumber, IMissionLog missionLog)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new System.ArgumentException("The machine's serial number cannot be empty.", nameof(serialNumber));
            }

            if (missionLog is null)
            {
                throw new System.ArgumentNullException(nameof(missionLog));
            }

            var machine = this.realm.All<Models.Machine>().SingleOrDefault(m => m.SerialNumber == serialNumber);
            if (machine is null)
            {
                throw new System.ArgumentException($"No machine corresponding to the serial '{serialNumber}' was found.");
            }

            var newId = 0;
            if (this.realm.All<Models.MissionLog>().OrderByDescending(e => e.Id).FirstOrDefault() is Models.MissionLog mission)
            {
                newId = mission.Id + 1;
            }

            var logEntry = new Models.MissionLog
            {
                Id = newId,
                Machine = machine,
                Bay = missionLog.Bay,
                CellId = missionLog.CellId,
                CreationDate = missionLog.CreationDate,
                //Destination = missionLog.,
                Direction = missionLog.Direction,
                EjectLoadUnit = missionLog.EjectLoadUnit,
                LoadUnitId = missionLog.LoadUnitId,
                MissionId = missionLog.MissionId,
                MissionType = missionLog.MissionType,
                Priority = missionLog.Priority,
                Stage = missionLog.Stage,
                Status = missionLog.Status,
                StopReason = missionLog.StopReason,
                TimeStamp = missionLog.TimeStamp,
                WmsId = missionLog.WmsId,
            };

            await this.realm.WriteAsync(r => r.Add(logEntry));
        }

        #endregion
    }
}
