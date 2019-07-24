namespace Ferretto.VW.App.Services.Models
{
    public enum MachineServiceStatus
    {
        NotSpecified,

        Valid = 'V',

        Expiring = 'G',

        Expired = 'X',
    }
}
