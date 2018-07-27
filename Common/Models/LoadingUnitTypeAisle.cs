namespace Ferretto.Common.Models
{
    // Tipo Udc - Corridoio
    public partial class LoadingUnitTypeAisle
    {
        public int AisleId { get; set; }
        public int LoadingUnitTypeId { get; set; }

        public Aisle Aisle { get; set; }
        public LoadingUnitType LoadingUnit { get; set; }
    }
}
