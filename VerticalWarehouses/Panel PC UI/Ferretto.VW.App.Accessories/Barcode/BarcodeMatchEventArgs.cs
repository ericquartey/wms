using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Devices.BarcodeReader;

namespace Ferretto.VW.App.Accessories
{
    public class BarcodeMatchEventArgs : BarcodeEventArgs
    {
        #region Constructors

        public BarcodeMatchEventArgs(string barcode, string userAction) : base(barcode)
        {
            this.UserAction = userAction;
        }

        #endregion

        #region Properties

        public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();

        public string UserAction { get; }

        #endregion
    }
}
