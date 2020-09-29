using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
using Microsoft.Extensions.Logging;
using Realms;

namespace Ferretto.VW.TelemetryService.Providers
{
    internal sealed class IOLogProvider : IIOLogProvider
    {
        #region Fields

        private readonly ILogger<IOLogProvider> logger;

        private readonly Realm realm;

        #endregion

        #region Constructors

        public IOLogProvider(Realm realm,
                          ILogger<IOLogProvider> logger)
        {
            this.realm = realm;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public void DeleteOldLogs(TimeSpan maximumLogTimespan)
        {
            this.logger.LogDebug("Deleting old IO logs ...");

            using var trans = this.realm.BeginWrite();

            var ioLogs = this.realm.All<Models.IOLog>();

            var lastLog = ioLogs.OrderByDescending(e => e.TimeStamp).FirstOrDefault();

            if (lastLog is null)
            {
                return;
            }

            var minTimestamp = lastLog.TimeStamp - maximumLogTimespan;

            var logsToDelete = ioLogs.Where(e => e.TimeStamp < minTimestamp);

            this.realm.RemoveRange(logsToDelete);

            trans.Commit();

            this.logger.LogDebug("A total of {count} IO logs were deleted.", logsToDelete.Count());
        }

        public IEnumerable<IIOLog> GetAll()
        {
            return this.realm.All<Models.IOLog>().ToArray();
        }

        public IEnumerable<IIOLog> GetByTimeStamp(string serialNumber, DateTimeOffset start, DateTimeOffset end)
        {
            return this.realm.All<Models.IOLog>().Where(io => io.TimeStamp >= start && io.TimeStamp <= end).ToArray();
        }

        public async Task SaveAsync(string serialNumber, IIOLog ioLog)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new System.ArgumentException("The machine's serial number cannot be empty.", nameof(serialNumber));
            }

            if (ioLog is null)
            {
                throw new System.ArgumentNullException(nameof(ioLog));
            }

            var machine = this.realm.All<Models.Machine>().SingleOrDefault(m => m.SerialNumber == serialNumber);
            if (machine is null)
            {
                throw new System.ArgumentException($"No machine corresponding to the serial '{serialNumber}' was found.");
            }

            var logEntry = new Models.IOLog
            {
                BayNumber = ioLog.BayNumber,
                Description = ioLog.Description,
                Input = ioLog.Input,
                Output = ioLog.Output,
                Machine = machine,
                TimeStamp = ioLog.TimeStamp,
            };

            await this.realm.WriteAsync(r => r.Add(logEntry));
        }

        #endregion
    }
}
