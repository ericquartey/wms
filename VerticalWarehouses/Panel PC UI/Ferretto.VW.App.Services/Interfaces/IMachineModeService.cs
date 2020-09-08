using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IMachineModeService
    {
        #region Properties

        bool IsWmsEnabled { get; }

        MachineMode MachineMode { get; }

        MachinePowerState MachinePower { get; }

        #endregion

        #region Methods

        Task OnUpdateServiceAsync();

        Task PowerOffAsync();

        Task PowerOnAsync();

        #endregion
    }
}
