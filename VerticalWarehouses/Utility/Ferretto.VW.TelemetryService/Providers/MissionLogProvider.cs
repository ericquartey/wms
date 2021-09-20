using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.TelemetryService.Data;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.TelemetryService.Providers
{
    internal sealed class MissionLogProvider : IMissionLogProvider
    {
        #region Fields

        private readonly IDataContext dataContext;

        private readonly ILogger<MissionLogProvider> logger;

        #endregion

        #region Constructors

        public MissionLogProvider(IDataContext dataContext,
                                  ILogger<MissionLogProvider> logger)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public void DeleteOldLogs(TimeSpan maximumLogTimespan)
        {
            lock (this.dataContext)
            {
                this.logger.LogDebug("Deleting old mission logs ...");

                var missionLogs = this.dataContext.MissionLogs.ToArray();

                var lastLog = missionLogs.LastOrDefault();

                if (lastLog is null)
                {
                    return;
                }

                var minTimestamp = lastLog.TimeStamp - maximumLogTimespan;

                var logsToDelete = missionLogs.Where(e => e.TimeStamp < minTimestamp);

                var countLogsToDelete = logsToDelete.Count();

                this.dataContext.MissionLogs.RemoveRange(logsToDelete);
                this.dataContext.SaveChanges();

                this.logger.LogDebug($"A total of {countLogsToDelete} mission logs were deleted.");
            }
        }

        public IEnumerable<IMissionLog> GetAll()
        {
            lock (this.dataContext)
            {
                return this.dataContext.MissionLogs.ToArray();
            }
        }

        public IEnumerable<Data.MissionLog> GetAllId()
        {
            lock (this.dataContext)
            {
                return this.dataContext.MissionLogs.ToArray();
            }
        }

        public void Remove(IEnumerable<Data.MissionLog> logsToDelete)
        {
            lock (this.dataContext)
            {
                this.dataContext.MissionLogs.RemoveRange(logsToDelete.ToArray());
                this.dataContext.SaveChanges();
            }
        }

        public void SaveAsync(string serialNumber, IMissionLog missionLog)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new System.ArgumentException("The machine's serial number cannot be empty.", nameof(serialNumber));
            }

            if (missionLog is null)
            {
                throw new System.ArgumentNullException(nameof(missionLog));
            }

            lock (this.dataContext)
            {
                var machine = this.dataContext.Machines.SingleOrDefault(m => m.SerialNumber == serialNumber);
                if (machine is null)
                {
                    throw new System.ArgumentException($"No machine corresponding to the serial '{serialNumber}' was found.");
                }

                var logEntry = new Data.MissionLog
                {
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

                this.dataContext.MissionLogs.Add(logEntry);
                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
