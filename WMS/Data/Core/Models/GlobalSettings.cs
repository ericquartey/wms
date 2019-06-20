using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.WMS.Data.Core.Models;

namespace Ferretto.WMS.Data.Core
{
    public class GlobalSettings
    {
        #region Properties

        public int Id { get; set; }

        public double MinStepCompartment { get; set; } = 5;

        #endregion
    }
}
