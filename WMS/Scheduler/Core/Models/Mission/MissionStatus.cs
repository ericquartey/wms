namespace Ferretto.WMS.Scheduler.Core.Models
{
    public enum MissionStatus
    {
        NotSpecified,

        New = 'N',

        Waiting = 'W',

        Executing = 'X',

        Completed = 'C',

        Error = 'E',

        Incomplete = 'I'
    }
}
