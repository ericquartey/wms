namespace Ferretto.WMS.Data.Core.Models
{
    public sealed class ItemCompartmentType : BaseModel<int>
    {
        #region Properties

        public int CompartmentsCount { get; set; }

        public int CompartmentTypeId { get; set; }

        public int EmptyCompartmentsCount { get; set; }

        public double? Height { get; set; }

        public int ItemId { get; set; }

        [Positive]
        public double? MaxCapacity { get; set; }

        public double? Width { get; set; }

        #endregion
    }
}
