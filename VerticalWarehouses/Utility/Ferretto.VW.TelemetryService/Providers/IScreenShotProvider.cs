using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Providers
{
    public interface IScreenShotProvider
    {
        #region Methods

        void DeleteOldLogs(TimeSpan maximumLogTimespan);

        IEnumerable<IScreenShot> GetAll();

        IEnumerable<Data.ScreenShot> GetAllId();

        void Remove(IEnumerable<Data.ScreenShot> logsToDelete);

        void SaveAsync(string serialNumber, IScreenShot screenshot);

        #endregion
    }
}
