namespace Ferretto.VW.AutomationService.Hubs
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
           "Major Code Smell",
           "S4261: Add the 'Async' suffix to the name of this method.",
           Justification = "The methods names here will be exposed by SignalR, so we want that names are clean")]
    public interface IMachineHub
    {
        #region Methods

        void EchoCurrentStatus(MachineStatus machine);

        void ElevatorPositionChanged(int position);

        void GetCurrentStatus();

        void LoadingUnitInBayChanged(int bayId, int? loadingUnitId);

        void LoadingUnitInElevatorChanged(int? loadingUnitId);

        void ModeChanged(MachineMode mode, int? faultCode);

        void UserChanged(int? userId, int bayId);

        #endregion
    }
}
