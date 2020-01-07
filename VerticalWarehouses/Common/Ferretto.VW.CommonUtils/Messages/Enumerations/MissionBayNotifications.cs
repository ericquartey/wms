using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    [JsonConverter(typeof(StringEnumConverter))]
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
