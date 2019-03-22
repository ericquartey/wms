namespace Ferretto.WMS.App.Core.Models
{
    public class ItemCompartmentType : BusinessObject
    {
        #region Properties

        public int CompartmentTypeId { get; set; }

        public int ItemId { get; set; }

        public int? MaxCapacity { get; set; }

        #endregion
    }
}
