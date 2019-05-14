using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ItemPutOptions
    {
        #region Properties

        public IEnumerable<Compartment> Compartments { get; set; }

        public IEnumerable<CompartmentPut> CompartmentsPut { get; set; }

        public ItemManagementType? ManagementType { get; set; }

        #endregion
    }
}
