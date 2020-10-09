using System.Net;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.Devices.LaserPointer
{
    public interface ILaserPointerDriver
    {
        #region Methods

        LaserPoint CalculateLaserPoint(double loadingUnitWidth, double loadingUnitDepth, double compartmentWidth, double compartmentDepth, double compartmentXPosition, double compartmentYPosition, double missionOperationItemHeight, bool isBayUpperPosition, WarehouseSide baySide);

        LaserPoint CalculateLaserPointForSocketLink(int x, int y, int z, bool isBayUpperPosition, WarehouseSide baySide);

        bool Configure(IPAddress ipAddress, int port, double xOffset = 0, double yOffset = 0, double zOffsetLowerPosition = 0, double zOffsetUpperPosition = 0);

        Task<bool> EnabledAsync(bool enable, bool onMovement);

        Task<bool> HelpAsync();

        Task<bool> HomeAsync();

        Task<bool> JogAsync(LaserPointerCommands.Command JogCommand);

        Task<bool> MoveAndSwitchOnAsync(LaserPoint point);

        Task<bool> MoveAsync(LaserPoint point);

        Task<bool> ParametersAsync();

        Task<bool> PositionAsync(LaserSetPosition position);

        Task<bool> PositionFinishAsync();

        Task<bool> PositionInitializeAsync();

        Task<bool> PositionSaveAsync();

        Task<bool> StepAsync(LaserStep spep);

        Task<bool> SwitchOnAndMoveAsync(LaserPoint point);

        Task<bool> TestAsync(bool value);

        #endregion
    }
}
