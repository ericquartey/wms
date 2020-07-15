using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Providers
{
    public interface IErrorLogProvider
    {
        #region Methods

        IEnumerable<IErrorLog> GetAll();

        Task SaveAsync(string serialNumber, IErrorLog machine);

        #endregion
    }
}
