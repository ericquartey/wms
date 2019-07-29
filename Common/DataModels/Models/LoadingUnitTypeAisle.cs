namespace Ferretto.Common.DataModels
{
    // Tipo Udc - Corridoio
    public sealed class LoadingUnitTypeAisle
    {
        #region Properties

        public Aisle Aisle { get; set; }

        public int AisleId { get; set; }

        public LoadingUnitType LoadingUnit { get; set; }

        public int LoadingUnitTypeId { get; set; }

        #endregion
    }
}
