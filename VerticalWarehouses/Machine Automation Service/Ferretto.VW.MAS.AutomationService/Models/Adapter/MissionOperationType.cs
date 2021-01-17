namespace Ferretto.VW.MAS.AutomationService.Models
{
    public enum MissionOperationType
    {
        NotSpecified,

        Bypass = 'B',

        Inventory = 'I',

        Pick = 'P',

        Put = 'T',

        LoadingUnitCheck = 'C',

        Reorder = 'O',

        Replace = 'R',
    }
}
