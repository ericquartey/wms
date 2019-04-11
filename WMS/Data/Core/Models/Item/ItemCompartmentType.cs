namespace Ferretto.WMS.Data.Core.Models
{
    public sealed class ItemCompartmentType : BaseModel<int>
    {
        #region Properties

        public int CompartmentTypeId { get; set; }

        public int ItemId { get; set; }

        public double? MaxCapacity { get; set; }

        #endregion
    }
}
