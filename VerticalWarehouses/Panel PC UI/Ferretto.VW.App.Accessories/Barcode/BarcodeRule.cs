using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ferretto.VW.App.Accessories
{
    public class BarcodeRule
    {
        #region Properties

        public string Name { get; set; }

        public IEnumerable<Regex> Patterns { get; set; }

        public UserAction UserAction { get; set; }

        #endregion
    }
}
