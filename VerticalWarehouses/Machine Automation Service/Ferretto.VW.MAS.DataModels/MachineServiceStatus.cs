namespace Ferretto.VW.MAS.AutomationService.Models
{
    public enum MachineServiceStatus
    {
        NotSpecified,

        Valid = 'V',

        Expiring = 'G',

        Expired = 'X',
    }
}
