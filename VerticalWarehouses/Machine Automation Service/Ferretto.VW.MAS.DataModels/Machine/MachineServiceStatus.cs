namespace Ferretto.VW.MAS.DataModels
{
    public enum MachineServiceStatus
    {
        Undefined,

        Valid = 'V',

        Expiring = 'G',

        Expired = 'X',

        Completed = 'C',
    }
}
