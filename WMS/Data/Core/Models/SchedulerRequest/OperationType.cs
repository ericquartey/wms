namespace Ferretto.WMS.Data.Core.Models
{
    public enum OperationType
    {
        NotSpecified,

        Insertion = 'I',

        Withdrawal = 'W',

        Replacement = 'R',

        Reorder = 'O'
    }
}
