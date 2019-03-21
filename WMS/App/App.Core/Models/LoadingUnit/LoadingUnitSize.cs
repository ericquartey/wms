using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.Common.BusinessModels
{
    public class LoadingUnitSize : BusinessObject
    {
        #region Properties

        public int Height { get; set; }

        public int Length { get; set; }

        public int Weight { get; set; }

        public int Width { get; set; }

        #endregion
    }
}
