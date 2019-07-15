namespace Ferretto.WMS.Data.Core.Models
{
    public enum OperationType
    {
        NotSpecified,

        Put = 'U',

        Pick = 'P',

        Replacement = 'R',

        Reorder = 'O'
    }
}
