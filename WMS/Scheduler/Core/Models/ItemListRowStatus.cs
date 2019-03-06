namespace Ferretto.WMS.Scheduler.Core.Models
{
    public enum ItemListRowStatus
    {
        NotSpecified,

        Waiting = 'W',

        Executing = 'E',

        Completed = 'C',

        Incomplete = 'I',

        Suspended = 'S'
    }
}
