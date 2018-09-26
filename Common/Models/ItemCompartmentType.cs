using System.Collections.Generic;

namespace Ferretto.Common.Models
{
    // Corridoio
    public sealed class ItemCompartmentType
    {
        #region Properties

        public CompartmentType CompartmentType { get; set; }
        public int CompartmentTypeId { get; set; }
        public Item Item { get; set; }
        public int ItemId { get; set; }
        public int? MaxCapacity { get; set; }

        #endregion Properties
    }
}
