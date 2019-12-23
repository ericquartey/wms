using System.Collections.Generic;
using System.Linq;

namespace Ferretto.VW.App.Accessories
{
    public class BarcodeContext
    {
        #region Properties

        public string Name { get; set; }

        public IEnumerable<BarcodeRule> Rules { get; set; }

        #endregion

        #region Methods

        public BarcodeRule Match(string barcode)
        {
            return this.Rules.FirstOrDefault(r => r.IsMatch(barcode));
        }

        #endregion
    }
}
