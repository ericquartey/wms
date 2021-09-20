using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.ServiceDesk.Telemetry;
using Ferretto.VW.TelemetryService.Data;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.TelemetryService.Providers
{
    internal sealed class ServicingInfoProvider : IServicingInfoProvider
    {
        #region Fields

        private readonly IDataContext dataContext;

        private readonly ILogger<ServicingInfoProvider> logger;

        #endregion

        #region Constructors

        public ServicingInfoProvider(IDataContext dataContext, ILogger<ServicingInfoProvider> logger)
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
                this.logger.LogDebug("Deleting old servicing info ...");

                var servicingInfo = this.dataContext.ServicingInfos.ToArray();

                var lastLog = servicingInfo.LastOrDefault();

                if (lastLog is null)
                {
                    return;
                }

                var minTimestamp = lastLog.TimeStamp - maximumLogTimespan;

                var logsToDelete = servicingInfo.Where(e => e.TimeStamp < minTimestamp);

                var countLogsToDelete = logsToDelete.Count();

                this.dataContext.ServicingInfos.RemoveRange(logsToDelete);
                this.dataContext.SaveChanges();

                this.logger.LogDebug($"A total of {countLogsToDelete} servicing info were deleted.");
            }
        }

        public IEnumerable<IServicingInfo> GetAll()
        {
            lock (this.dataContext)
            {
                return this.dataContext.ServicingInfos.ToArray();
            }
        }

        public IEnumerable<Data.ServicingInfo> GetAllId()
        {
            lock (this.dataContext)
            {
                return this.dataContext.ServicingInfos.ToArray();
            }
        }

        public void Remove(IEnumerable<Data.ServicingInfo> logsToDelete)
        {
            lock (this.dataContext)
            {
                this.dataContext.ServicingInfos.RemoveRange(logsToDelete.ToArray());
                this.dataContext.SaveChanges();
            }
        }

        public void SaveAsync(string serialNumber, IServicingInfo servicingInfo)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new System.ArgumentException("The machine's serial number cannot be empty.", nameof(serialNumber));
            }

            if (servicingInfo is null)
            {
                throw new System.ArgumentNullException(nameof(servicingInfo));
            }

            lock (this.dataContext)
            {
                var machine = this.dataContext.Machines.SingleOrDefault(m => m.SerialNumber == serialNumber);
                if (machine is null)
                {
                    throw new System.ArgumentException($"No machine corresponding to the serial '{serialNumber}' was found.");
                }

                var servicingInfoEntity = new Data.ServicingInfo
                {
                    InstallationDate = servicingInfo.InstallationDate,
                    IsHandOver = servicingInfo.IsHandOver,
                    LastServiceDate = servicingInfo.LastServiceDate,
                    NextServiceDate = servicingInfo.NextServiceDate,
                    ServiceStatusId = servicingInfo.ServiceStatusId,
                    TimeStamp = servicingInfo.TimeStamp,
                    TotalMissions = servicingInfo.TotalMissions
                };

                this.dataContext.ServicingInfos.Add(servicingInfoEntity);
                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
