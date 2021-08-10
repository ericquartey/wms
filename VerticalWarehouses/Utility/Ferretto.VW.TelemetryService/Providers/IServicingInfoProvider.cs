using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Providers
{
    public interface IServicingInfoProvider
    {
        #region Methods

        void DeleteOldLogs(TimeSpan maximumLogTimespan);

        IEnumerable<IServicingInfo> GetAll();

        Task SaveAsync(string serialNumber, IServicingInfo servicingInfo);

        #endregion
    }
}
