using System.Threading.Tasks;
using Ferretto.VW.TelemetryService.Models;

namespace Ferretto.VW.TelemetryService.Provider
{
    public interface IMachineProvider
    {
        #region Methods

        Task<Machine> GetAsync();

        Task SaveAsync(Machine machine);

        #endregion
    }
}
