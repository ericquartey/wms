namespace Ferretto.WMS.Data.Core.Models
{
    public enum MaintenanceStatus
    {
        NotSpecified,

        Valid = 'V',

        Expiring = 'G',

        Expired = 'X',
    }
}
