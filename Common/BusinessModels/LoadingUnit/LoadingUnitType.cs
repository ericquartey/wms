using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.Common.BusinessModels
{
    public class LoadingUnitType
    {
        #region Properties

        public string Description { get; set; }

        public int HasCompartments { get; set; }

        public int LoadingUnitHeightClassId { get; set; }

        public int LoadingUnitSizeClassId { get; set; }

        public int LoadingUnitWeightClassId { get; set; }

        #endregion Properties
    }
}
