using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Devices.LaserPointer
{
    public interface ILaserPointerDriver
    {
        #region Methods

        bool Configure(IPAddress ipAddress, int port);

        Task<bool> EnabledAsync(bool enable, bool onMovement);

        Task<bool> HelpAsync();

        Task<bool> HomeAsync();

        Task<bool> JogAsync(LaserPointerCommands.Command JogCommand);

        Task<bool> MoveAsync(LaserPoint point);

        Task<bool> PositionAsync(LaserSetPosition position);

        Task<bool> PositionFinishAsync();

        Task<bool> PositionInitializeAsync();

        Task<bool> PositionSaveAsync();

        Task<bool> StepAsync(LaserStep spep);

        Task<bool> TestAsync(bool value);

        #endregion
    }
}
