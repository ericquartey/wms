using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Providers
{
    public interface IErrorLogProvider
    {
        #region Methods

        void DeleteOldLogs(System.TimeSpan maximumLogTimespan);

        IEnumerable<IErrorLog> GetAll();

        IEnumerable<Data.ErrorLog> GetAllId();

        void Remove(IEnumerable<Data.ErrorLog> logsToDelete);

        void SaveAsync(string serialNumber, IErrorLog machine);

        #endregion
    }
}
