using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Common_Utils
{
    public class ConnectedClient
    {
        #region Constructors

        public ConnectedClient(string connectionID)
        {
            this.ConnectionID = connectionID;
        }

        #endregion

        #region Properties

        public string ConnectionID { get; set; }

        #endregion
    }
}
