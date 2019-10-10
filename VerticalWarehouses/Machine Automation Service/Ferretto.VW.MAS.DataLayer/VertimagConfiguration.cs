using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer
{
    public class VertimagConfiguration
    {
        #region Properties

        public Machine Machine { get; set; }

        public SetupProceduresSet SetupProcedures { get; set; }

        #endregion
    }
}
