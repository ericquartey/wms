using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum HorizontalMovementDirection
    {
        NotSpecified,

        Forwards,

        Backwards,
    }
}
