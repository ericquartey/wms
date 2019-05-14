using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.WMS.Data.Core.Models
{
    public class CompartmentPut : BaseModel<int>
    {
        #region Properties

        public int? CompartmentTypeId { get; set; }

        public DateTime FifoStartDate { get; set; }

        public double FifoTime { get; set; }

        public double MaxCapacity { get; set; }

        public double QuantityLeftToReserve { get; set; }

        public double RequestedQuantity { get; set; }

        public double ReservedToPut { get; set; }

        public double Stock { get; set; }

        #endregion
    }
}
