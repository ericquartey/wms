namespace Ferretto.WMS.Data.Core.Models
{
    public enum ItemListStatus
    {
        NotSpecified,

        New = 'N',

        Ready = 'R',

        Waiting = 'W',

        Executing = 'X',

        Completed = 'C',

        Error = 'E',

        Incomplete = 'I',

        Suspended = 'S',
    }
}
