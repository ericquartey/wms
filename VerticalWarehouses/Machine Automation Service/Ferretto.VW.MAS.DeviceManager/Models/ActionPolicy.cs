using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DeviceManager
{
    public class ActionPolicy
    {
        #region Fields

        public static readonly ActionPolicy Allowed = new ActionPolicy { IsAllowed = true };

        #endregion

        #region Properties

        public bool IsAllowed { get; set; }

        public string Reason { get; set; }

        public string ReasonType { get; set; }

        #endregion
    }
}
