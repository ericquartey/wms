namespace Ferretto.WMS.Scheduler.Core.Models
{
    public enum ItemListStatus
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
