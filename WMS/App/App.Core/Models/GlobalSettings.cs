using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.Utils;

namespace Ferretto.WMS.App.Core.Models
{
    [Resource(nameof(GlobalSettings))]
    public class GlobalSettings
    {
        #region Properties

        public int Id { get; set; }

        public double MinStepCompartment { get; set; }

        #endregion
    }
}
