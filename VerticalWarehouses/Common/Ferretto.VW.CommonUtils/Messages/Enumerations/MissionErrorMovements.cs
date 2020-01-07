using System;

namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    [Flags]
    public enum MissionErrorMovements
    {
        None = 0,

        MoveForward = 1,

        MoveBackward = 2,

        MoveShutter = 4,
    }
}
