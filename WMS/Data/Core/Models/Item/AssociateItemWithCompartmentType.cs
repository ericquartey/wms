using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class AssociateItemWithCompartmentType : BaseModel<int>, IItemCompartmentTypeDeletePolicy
    {
        #region Properties

        public string AbcClassDescription { get; set; }

        public string AbcClassId { get; set; }

        public string Code { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        public string ItemCategoryDescription { get; set; }

        public int? ItemCategoryId { get; set; }

        public double? MaxCapacity { get; set; }

        public string MeasureUnitDescription { get; set; }

        public double TotalAvailable { get; set; }

        public double TotalReservedForPick { get; set; }

        public double TotalReservedToPut { get; set; }

        public double TotalStock { get; set; }

        #endregion
    }
}
