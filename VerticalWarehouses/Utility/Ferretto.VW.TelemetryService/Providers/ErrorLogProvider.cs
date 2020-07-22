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
    internal sealed class ErrorLogProvider : IErrorLogProvider
    {
        #region Fields

        private readonly ILogger<ErrorLogProvider> logger;

        private readonly Realm realm;

        #endregion

        #region Constructors

        public ErrorLogProvider(Realm realm,
                                ILogger<ErrorLogProvider> logger)
        {
            this.realm = realm;
            this.logger = logger;
        }

        #endregion

        #region Methods

        public void DeleteOldLogsAsync(TimeSpan maximumLogTimespan)
        {
            this.logger.LogDebug("Deleting old error logs ...");

            var realm = Realm.GetInstance();

            using var trans = realm.BeginWrite();

            var errorLogs = realm.All<Models.ErrorLog>();

            var lastLog = errorLogs.OrderByDescending(e => e.OccurrenceDate).FirstOrDefault();

            if (lastLog is null)
            {
                return;
            }

            var logsToDelete = errorLogs.OrderByDescending(e => e.OccurrenceDate)
                                        .Where(e => (e.OccurrenceDate - maximumLogTimespan) < lastLog.OccurrenceDate);

            realm.RemoveRange<Models.ErrorLog>(logsToDelete);

            trans.Commit();

            this.logger.LogDebug("A total of {count} error logs were deleted.", logsToDelete.Count());
        }

        public IEnumerable<IErrorLog> GetAll()
        {
            return this.realm.All<Models.ErrorLog>().ToArray();
        }

        public async Task SaveAsync(string serialNumber, IErrorLog errorLog)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new System.ArgumentException("The machine's serial number cannot be empty.", nameof(serialNumber));
            }

            if (errorLog is null)
            {
                throw new System.ArgumentNullException(nameof(errorLog));
            }

            var machine = this.realm.All<Models.Machine>().SingleOrDefault(m => m.SerialNumber == serialNumber);
            if (machine is null)
            {
                throw new System.ArgumentException($"No machine corresponding to the serial '{serialNumber}' was found.");
            }

            var newId = 0;
            if (this.realm.All<Models.ErrorLog>().OrderByDescending(e => e.Id).FirstOrDefault() is Models.ErrorLog error)
            {
                newId = error.Id + 1;
            }

            var logEntry = new Models.ErrorLog
            {
                Id = newId,
                Machine = machine,
                AdditionalText = errorLog.AdditionalText,
                BayNumber = errorLog.BayNumber,
                Code = errorLog.Code,
                DetailCode = errorLog.DetailCode,
                OccurrenceDate = errorLog.OccurrenceDate,
                ResolutionDate = errorLog.ResolutionDate
            };

            await this.realm.WriteAsync(r => r.Add(logEntry));
        }

        #endregion
    }
}
