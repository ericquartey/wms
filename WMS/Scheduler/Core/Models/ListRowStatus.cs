namespace Ferretto.WMS.Scheduler.Core.Models
{
    public enum ListRowStatus
    {
        NotSpecified,

        Waiting = 'W',

        Executing = 'E',

        Completed = 'C',

        Incomplete = 'I',

        Suspended = 'S'
    }
}
