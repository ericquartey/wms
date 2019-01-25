namespace Ferretto.WMS.Scheduler.Core
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
