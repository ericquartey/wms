using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ferretto.VW.MAS.DataModels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AlphaNumericBarSize
    {
        ExtraSmall = 51,

        Small = 64,

        Medium = 80,

        Large = 96,

        ExtraLarge = 112
    }
}
