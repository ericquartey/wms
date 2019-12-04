using System.Threading.Tasks;
using Ferretto.VW.App.Services.Models;

namespace Ferretto.VW.App.Services
{
    public interface IMachineService
    {
        #region Properties

        bool IsHoming { get; }

        MachineStatus MachineStatus { get; }

        #endregion

        #region Methods

        Task StopMovingByAllAsync();

        #endregion
    }
}
