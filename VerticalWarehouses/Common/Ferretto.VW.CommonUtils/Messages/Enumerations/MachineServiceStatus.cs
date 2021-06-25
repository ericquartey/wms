namespace Ferretto.VW.CommonUtils.Messages.Enumerations
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
