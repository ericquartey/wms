namespace Ferretto.WMS.Data.Core.Models
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
