using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.TelemetryService.Data;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.TelemetryService.Providers
{
    internal sealed class ScreenShotProvider : IScreenShotProvider
    {
        #region Fields

        private readonly IDataContext dataContext;

        private readonly ILogger<ErrorLogProvider> logger;

        #endregion

        #region Constructors

        public ScreenShotProvider(IDataContext dataContext,
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
                this.logger.LogDebug("Deleting old screenShot ...");

                var screenShotLogs = this.dataContext.ScreenShots.ToArray();

                var lastLog = screenShotLogs.LastOrDefault();

                if (lastLog is null)
                {
                    return;
                }

                var minTimestamp = lastLog.TimeStamp - maximumLogTimespan;

                var logsToDelete = screenShotLogs.Where(e => e.TimeStamp < minTimestamp);

                var countLogsToDelete = logsToDelete.Count();

                this.dataContext.ScreenShots.RemoveRange(logsToDelete);
                this.dataContext.SaveChanges();

                this.logger.LogDebug($"A total of {countLogsToDelete} screenShot were deleted.");
            }
        }

        public IEnumerable<IScreenShot> GetAll()
        {
            lock (this.dataContext)
            {
                return this.dataContext.ScreenShots.ToArray();
            }
        }

        public IEnumerable<Data.ScreenShot> GetAllId()
        {
            lock (this.dataContext)
            {
                return this.dataContext.ScreenShots.ToArray();
            }
        }

        public void Remove(IEnumerable<Data.ScreenShot> logsToDelete)
        {
            lock (this.dataContext)
            {
                this.dataContext.ScreenShots.RemoveRange(logsToDelete.ToArray());
                this.dataContext.SaveChanges();
            }
        }

        public void SaveAsync(string serialNumber, IScreenShot screenShot)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new System.ArgumentException("The machine's serial number cannot be empty.", nameof(serialNumber));
            }

            if (screenShot is null)
            {
                throw new System.ArgumentNullException(nameof(screenShot));
            }

            lock (this.dataContext)
            {
                var machine = this.dataContext.Machines.SingleOrDefault(m => m.SerialNumber == serialNumber);
                if (machine is null)
                {
                    throw new System.ArgumentException($"No machine corresponding to the serial '{serialNumber}' was found.");
                }

                var screenShotEntry = new Data.ScreenShot
                {
                    BayNumber = screenShot.BayNumber,
                    Machine = machine,
                    Image = screenShot.Image,
                    TimeStamp = screenShot.TimeStamp,
                    ViewName = screenShot.ViewName,
                };

                this.dataContext.ScreenShots.Add(screenShotEntry);
                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
