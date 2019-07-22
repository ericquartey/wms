namespace Ferretto.VW.MAS.DataModels
{
    public enum MachineServiceStatus
    {
        NotSpecified,

        Valid = 'V',

        Expiring = 'G',

        Expired = 'X',
    }
}
