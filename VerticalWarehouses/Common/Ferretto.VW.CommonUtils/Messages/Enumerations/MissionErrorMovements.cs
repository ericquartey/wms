using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    [JsonConverter(typeof(StringEnumConverter))]
    [Flags]
    public enum MissionErrorMovements
    {
        None = 0,

        MoveForward = 1,

        MoveBackward = 2,

        MoveShutterOpen = 4,

        MoveShutterClosed = 8,

        AbortMovement = 16,

        HomeMovement = 32,
    }
}
