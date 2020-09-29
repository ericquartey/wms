using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Providers
{
    public interface IIOLogProvider
    {
        #region Methods

        void DeleteOldLogs(TimeSpan maximumLogTimespan);

        IEnumerable<IIOLog> GetAll();

        IEnumerable<IIOLog> GetByTimeStamp(string serialNumber, DateTimeOffset start, DateTimeOffset end);

        Task SaveAsync(string serialNumber, IIOLog ioLog);

        #endregion
    }
}
