using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DeviceManager
{
    public class ActionPolicy
    {
        #region Properties

        public bool IsAllowed { get; set; }

        public string Reason { get; set; }

        #endregion
    }
}
