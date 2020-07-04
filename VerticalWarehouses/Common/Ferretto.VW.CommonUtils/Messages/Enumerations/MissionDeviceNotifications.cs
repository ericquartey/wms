using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    [JsonConverter(typeof(StringEnumConverter))]
    [Flags]
    public enum MissionDeviceNotifications
    {
        None = 0,

        Positioning = 1,

        Shutter = 2,

        Homing = 4,

        CombinedMovements = 8
    }
}
