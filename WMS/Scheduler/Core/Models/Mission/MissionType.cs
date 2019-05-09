namespace Ferretto.WMS.Scheduler.Core.Models
{
    public enum MissionType
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
