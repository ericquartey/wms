using System.Threading.Tasks;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.VW.MachineAutomationService.Hubs
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
           "Major Code Smell",
           "S4261: Add the 'Async' suffix to the name of this method.",
           Justification = "The methods names here will be exposed by SignalR, so we want that names are clean")]
    public interface IMachineHub
    {
        #region Methods

        Task EchoCurrentStatus(MachineStatus machine);

        Task ElevatorPositionChanged(decimal position);

        Task GetCurrentStatus();

        Task LoadingUnitInBayChanged(int bayId, int? loadingUnitId);

        Task LoadingUnitInElevatorChanged(int? loadingUnitId);

        Task ModeChanged(Enums.MachineStatus mode, int? faultCode);

        Task UserChanged(int? userId, int bayId);

        #endregion
    }
}
