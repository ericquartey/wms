using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.Devices.LaserPointer
{
    public interface ILaserPointerDriver
    {
        #region Methods

        LaserPoint CalculateLaserPoint(double loadingUnitWidth, double loadingUnitDepth, double compartmentXPosition, double compartmentYPosition, bool isBayLowerPosition, WarehouseSide baySide);

        bool Configure(IPAddress ipAddress, int port, double yOffset = 0, double zOffsetLowerPosition = 0, double zOffsetUpperPosition = 0);

        Task<bool> EnabledAsync(bool enable, bool onMovement);

        Task<bool> HelpAsync();

        Task<bool> HomeAsync();

        Task<bool> IsConnectedAsync();

        Task<bool> JogAsync(LaserPointerCommands.Command JogCommand);

        Task<bool> MoveAsync(LaserPoint point);

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
