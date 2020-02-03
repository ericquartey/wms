using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IMachineModeService
    {
        #region Properties

        MachineMode MachineMode { get; }

        MachinePowerState MachinePower { get; }

        Task OnUpdateServiceAsync();

        #endregion

        #region Methods

        Task PowerOffAsync();

        Task PowerOnAsync();

        #endregion
    }
}
