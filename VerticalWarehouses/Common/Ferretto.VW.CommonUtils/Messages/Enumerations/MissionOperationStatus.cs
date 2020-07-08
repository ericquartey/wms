using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ferretto.VW.CommonUtils.Messages.Enumerations
{
    // TODO (used only for OnHold) substitute this enum with Ferretto.WMS.Data.WebAPI.Contracts.MissionOperationStatus
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MissionOperationStatus
    {
        NotSpecified = 0,

        Executing = 69,

        OnHold = 72,

        Completed = 84,
    }
}
