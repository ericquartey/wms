namespace Ferretto.Common.DataModels
{
    // Articolo - Tipo Scomparto
    public sealed class ItemCompartmentType
    {
        #region Properties

        public CompartmentType CompartmentType { get; set; }

        public int CompartmentTypeId { get; set; }

        public Item Item { get; set; }

        public int ItemId { get; set; }

        public int? MaxCapacity { get; set; }

        #endregion
    }
}
