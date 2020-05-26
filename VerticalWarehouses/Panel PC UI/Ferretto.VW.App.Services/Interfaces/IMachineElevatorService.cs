using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface IMachineElevatorService
    {
        #region Properties

        ElevatorPosition Position { get; }

        #endregion
    }
}
