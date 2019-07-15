namespace Ferretto.WMS.Data.Core.Models
{
    public enum MissionStatus
    {
        NotSpecified,

        New = 'N',

        Executing = 'X',

        Completed = 'C',

        Error = 'E',

        Incomplete = 'I',
    }
}
