namespace Ferretto.VW.MAS.AutomationService.Models
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
