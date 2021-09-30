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

        IEnumerable<Data.IOLog> GetAllId();

        IEnumerable<IIOLog> GetByTimeStamp(string serialNumber, DateTimeOffset start, DateTimeOffset end);

        void Remove(IEnumerable<Data.IOLog> logsToDelete);

        void SaveAsync(string serialNumber, IIOLog ioLog);

        #endregion
    }
}
