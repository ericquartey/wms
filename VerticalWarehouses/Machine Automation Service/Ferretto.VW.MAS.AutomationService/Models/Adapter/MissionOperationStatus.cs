namespace Ferretto.VW.MAS.AutomationService.Models
{
    public enum MissionOperationStatus
    {
        NotSpecified,

        New = 'N',

        Executing = 'X',

        Completed = 'C',

        Error = 'E',

        Incomplete = 'I',
    }
}
