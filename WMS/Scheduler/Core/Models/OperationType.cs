namespace Ferretto.WMS.Scheduler.Core.Models
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
