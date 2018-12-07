namespace Ferretto.WMS.Scheduler.Core
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
