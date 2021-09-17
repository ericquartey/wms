using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.TelemetryService.Data;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.TelemetryService.Providers
{
    internal sealed class IOLogProvider : IIOLogProvider
    {
        #region Fields

        private readonly IDataContext dataContext;

        private readonly ILogger<IOLogProvider> logger;

        #endregion

        #region Constructors

        public IOLogProvider(IDataContext dataContext,
                          ILogger<IOLogProvider> logger)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public void DeleteOldLogs(TimeSpan maximumLogTimespan)
        {
            this.logger.LogDebug("Deleting old IO logs ...");

            var ioLogs = this.dataContext.IOLogs;

            var lastLog = ioLogs.OrderByDescending(e => e.TimeStamp).FirstOrDefault();

            if (lastLog is null)
            {
                return;
            }

            var minTimestamp = lastLog.TimeStamp - maximumLogTimespan;

            var logsToDelete = ioLogs.Where(e => e.TimeStamp < minTimestamp);

            var countLogsToDelete = logsToDelete.Count();

            this.dataContext.IOLogs.RemoveRange(logsToDelete);
            this.dataContext.SaveChanges();

            this.logger.LogDebug($"A total of {countLogsToDelete} IO logs were deleted.");
        }

        public IEnumerable<IIOLog> GetAll()
        {
            return this.dataContext.IOLogs.ToArray();
        }

        public IEnumerable<IIOLog> GetByTimeStamp(string serialNumber, DateTimeOffset start, DateTimeOffset end)
        {
            return this.dataContext.IOLogs.Where(io => io.TimeStamp >= start && io.TimeStamp <= end).ToArray();
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

            var machine = this.dataContext.Machines.SingleOrDefault(m => m.SerialNumber == serialNumber);
            if (machine is null)
            {
                throw new System.ArgumentException($"No machine corresponding to the serial '{serialNumber}' was found.");
            }

            var logEntry = new Data.IOLog
            {
                BayNumber = ioLog.BayNumber,
                Description = ioLog.Description,
                Input = ioLog.Input,
                Output = ioLog.Output,
                Machine = machine,
                TimeStamp = ioLog.TimeStamp,
            };

            this.dataContext.IOLogs.Add(logEntry);
            this.dataContext.SaveChanges();
        }

        #endregion
    }
}
