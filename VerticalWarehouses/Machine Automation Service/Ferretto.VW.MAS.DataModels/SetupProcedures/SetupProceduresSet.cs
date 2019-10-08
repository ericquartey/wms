using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class SetupProceduresSet : DataModel
    {
        #region Properties

        public RepeatedTestProcedure ShutterTest { get; set; }

        #endregion
    }
}
