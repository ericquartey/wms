using System.Collections.Generic;
using System.Linq;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Accessories
{
    public class BarcodeContext
    {
        #region Properties

        public string Name { get; set; }

        public IEnumerable<BarcodeRule> Rules { get; set; }

        #endregion
    }
}
