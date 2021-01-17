namespace Ferretto.VW.MAS.AutomationService.Models
{
    public enum SchedulerRequestType
    {
        NotSpecified,

        Item = 'I',

        LoadingUnit = 'U',

        ItemList = 'L',

        ItemListRow = 'R',
    }
}
