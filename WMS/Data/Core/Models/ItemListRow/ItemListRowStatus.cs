namespace Ferretto.WMS.Data.Core.Models
{
    public enum ItemListRowStatus
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
