using System.Threading.Tasks;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Providers
{
    public interface IMachineProvider
    {
        #region Methods

        IMachine? Get();

        Task SaveAsync(IMachine machine);

        #endregion
    }
}
