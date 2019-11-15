using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IMachineModeService
    {
        #region Properties

        MachineMode MachineMode { get; }

        MachineMovementMode MachineMovementMode { get; }

        MachinePowerState MachinePower { get; }

        #endregion

        #region Methods

        Task PowerOffAsync();

        Task PowerOnAsync();

        #endregion
    }
}
