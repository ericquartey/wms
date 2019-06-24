namespace Ferretto.WMS.Data.Core.Models
{
    public enum MissionOperationType
    {
        NotSpecified,

        Bypass = 'B',

        Inventory = 'I',

        Pick = 'P',

        Put = 'T',

        Reorder = 'O',

        Replace = 'R'
    }
}
