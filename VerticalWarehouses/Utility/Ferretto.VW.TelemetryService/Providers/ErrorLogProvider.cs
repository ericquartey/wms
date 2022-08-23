using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.TelemetryService.Data;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.TelemetryService.Providers
{
    internal sealed class ErrorLogProvider : IErrorLogProvider
    {
        #region Fields

        private readonly IDataContext dataContext;

        private readonly ILogger<ErrorLogProvider> logger;

        #endregion

        #region Constructors

        public ErrorLogProvider(IDataContext dataContext,
                                ILogger<ErrorLogProvider> logger)
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
                this.logger.LogDebug("Deleting old error logs ...");

                var errorLogs = this.dataContext.ErrorLogs.ToArray();

                var lastLog = errorLogs.LastOrDefault();

                if (lastLog is null)
                {
                    return;
                }

                var minTimestamp = lastLog.OccurrenceDate - maximumLogTimespan;

                var logsToDelete = errorLogs.Where(e => e.OccurrenceDate < minTimestamp);

                var countLogsToDelete = logsToDelete.Count();

                this.dataContext.ErrorLogs.RemoveRange(logsToDelete);
                this.dataContext.SaveChanges();

                this.logger.LogDebug($"A total of {countLogsToDelete} error logs were deleted.");
            }
        }

        public IEnumerable<IErrorLog> GetAll()
        {
            lock (this.dataContext)
            {
                return this.dataContext.ErrorLogs.ToArray();
            }
        }

        public IEnumerable<Data.ErrorLog> GetAllId()
        {
            lock (this.dataContext)
            {
                return this.dataContext.ErrorLogs.ToArray();
            }
        }

        public void Remove(IEnumerable<Data.ErrorLog> logsToDelete)
        {
            lock (this.dataContext)
            {
                this.dataContext.ErrorLogs.RemoveRange(logsToDelete.ToArray());
                this.dataContext.SaveChanges();
            }
        }

        public void SaveAsync(string serialNumber, IErrorLog errorLog)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new ArgumentException("The machine's serial number cannot be empty.", nameof(serialNumber));
            }

            if (errorLog is null)
            {
                throw new ArgumentNullException(nameof(errorLog));
            }

            lock (this.dataContext)
            {
                var machine = this.dataContext.Machines.SingleOrDefault(m => m.SerialNumber == serialNumber);
                if (machine is null)
                {
                    throw new ArgumentException($"No machine corresponding to the serial '{serialNumber}' was found.");
                }

                var logEntry = new Data.ErrorLog()
                {
                    Machine = machine,
                    AdditionalText = errorLog.AdditionalText,
                    BayNumber = errorLog.BayNumber,
                    Code = errorLog.Code,
                    DetailCode = errorLog.DetailCode,
                    OccurrenceDate = errorLog.OccurrenceDate,
                    ResolutionDate = errorLog.ResolutionDate,
                    ErrorId = errorLog.ErrorId,
                };

                this.dataContext.ErrorLogs.Add(logEntry);
                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
