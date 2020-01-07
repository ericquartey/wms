using System;

namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    [Flags]
    public enum MissionBayNotifications
    {
        None = 0,

        BayOne = 1,

        BayTwo = 2,

        BayThree = 4,

        ElevatorBay = 8,
    }
}
