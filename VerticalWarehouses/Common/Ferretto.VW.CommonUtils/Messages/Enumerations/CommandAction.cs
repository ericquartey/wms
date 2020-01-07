using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ferretto.VW.CommonUtils
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CommandAction
    {
        Start,

        Activate,

        Pause,

        Resume,

        Abort,

        Stop,
    }
}
