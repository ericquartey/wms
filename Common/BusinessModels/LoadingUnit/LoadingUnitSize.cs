using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.Common.BusinessModels
{
    public class LoadingUnitSize : BusinessObject
    {
        #region Properties

        public double Height { get; set; }

        public double Length { get; set; }

        public int Weight { get; set; }

        public double Width { get; set; }

        #endregion
    }
}
