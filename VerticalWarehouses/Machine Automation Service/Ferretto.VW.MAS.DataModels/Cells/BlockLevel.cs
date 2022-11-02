using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ferretto.VW.MAS.DataModels
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BlockLevel
    {
        Undefined,

        // the cell is not blocked.
        None,

        // no load unit can be placed on the cell, but a load unit content can occupy the space of the cell.
        SpaceOnly,

        // no load unit or its content can be placed on the cell. No load unit content can occupy the space of the cell.
        Blocked,

        // the cell must be tested in the "first load unit" test. After testing the BlockLevel returns to None.
        NeedsTest,

        // the cell must be free because the next cell is occupied by a very heavy LU
        UnderWeight,

        // the cell is reserved for fixed-cell load units
        Reserved
    }
}
