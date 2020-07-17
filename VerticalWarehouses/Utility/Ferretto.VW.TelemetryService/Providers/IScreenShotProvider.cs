using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Providers
{
    public interface IScreenShotProvider
    {
        #region Methods

        IEnumerable<IScreenShot> GetAll();

        Task SaveAsync(string serialNumber, IScreenShot screenshot);

        #endregion
    }
}
