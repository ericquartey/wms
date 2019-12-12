using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface ISensorsService
    {
        #region Properties

        double? BayChainPosition { get; }

        double? ElevatorHorizontalPosition { get; }

        string ElevatorLogicalPosition { get; }

        double? ElevatorVerticalPosition { get; }

        LoadingUnit EmbarkedLoadingUnit { get; }

        bool IsLoadingUnitInBay { get; }

        bool IsLoadingUnitInMiddleBottomBay { get; }

        bool IsLoadingUnitOnElevator { get; }

        bool IsZeroChain { get; }

        string LoadingUnitPositionDownInBayCode { get; }

        string LoadingUnitPositionUpInBayCode { get; }

        string LogicalPosition { get; }

        string LogicalPositionId { get; }

        Sensors Sensors { get; }

        ShutterSensors ShutterSensors { get; }

        #endregion

        #region Methods

        Task RefreshAsync(bool forceRefresh);

        void RetrieveElevatorPosition(ElevatorPosition position);

        #endregion
    }
}
