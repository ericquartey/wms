using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Devices.BarcodeReader.Newland
{
    public abstract class BarcodeCommand
    {
        #region Constructors

        public BarcodeCommand(string commandCode)
        {
            this.CommandCode = commandCode;
        }

        #endregion

        #region Properties

        public string CommandCode { get; }

        #endregion
    }
}
