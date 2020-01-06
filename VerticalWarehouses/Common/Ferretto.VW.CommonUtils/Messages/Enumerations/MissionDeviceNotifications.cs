using System;

namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    [Flags]
    public enum MissionDeviceNotifications
    {
        None = 0,

        Positioning = 1,

        Shutter = 2,

        Homing = 4,
    }
}
