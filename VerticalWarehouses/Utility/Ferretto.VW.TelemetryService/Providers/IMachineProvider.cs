using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Providers
{
    public interface IMachineProvider
    {
        #region Methods

        IMachine? Get();

        void SaveAsync(IMachine machine);

        void SaveRawDatabaseContent(byte[]? rawDatabaseContent);

        #endregion
    }
}
