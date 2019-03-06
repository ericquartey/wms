namespace Ferretto.WMS.Scheduler.Core.Models
{
    public enum ListStatus
    {
        NotSpecified,

        Waiting = 'W',

        Executing = 'E',

        Completed = 'C',

        Incomplete = 'I',

        Suspended = 'S'
    }
}
