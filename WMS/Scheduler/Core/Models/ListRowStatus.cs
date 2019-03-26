namespace Ferretto.WMS.Scheduler.Core.Models
{
    public enum ListRowStatus
    {
        NotSpecified,

        New = 'N',

        Waiting = 'W',

        Executing = 'X',

        Completed = 'C',

        Error = 'E',

        Incomplete = 'I',

        Suspended = 'S'
    }
}
