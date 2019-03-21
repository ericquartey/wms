using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS_DataLayer.Enumerations
{
    public enum HorizontalAxisEnum : long
    {
        MaxSpeed = 0L,

        MaxAcceleration = 1L,

        MaxDeceleration = 2L,

        HomingExecuted = 3L,

        Offset = 4L,

        ClockWiseRun = 5L,

        AntiClockWiseRun = 6L,
    }
}
