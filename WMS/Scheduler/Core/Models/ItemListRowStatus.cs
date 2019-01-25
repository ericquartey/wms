namespace Ferretto.WMS.Scheduler.Core
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
