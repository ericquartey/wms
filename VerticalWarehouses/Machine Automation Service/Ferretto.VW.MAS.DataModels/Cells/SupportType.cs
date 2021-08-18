using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ferretto.VW.MAS.DataModels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SupportType
    {
        Undefined,

        Insert,

        Above
    }
}
