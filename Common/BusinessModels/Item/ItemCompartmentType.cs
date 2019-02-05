using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.Common.BusinessModels
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
