namespace Ferretto.VW.App.Services
{
    public enum MachineServiceStatus
    {
        NotSpecified,

        Valid = 'V',

        Expiring = 'G',

        Expired = 'X',
    }
}
