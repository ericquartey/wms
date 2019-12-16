using System.Collections.Generic;

namespace Ferretto.VW.MAS.DataModels
{
    public class VertimagConfiguration
    {
        #region Properties

        //public IEnumerable<LoadingUnit> LoadingUnits { get; set; }

        public Machine Machine { get; set; }

        public SetupProceduresSet SetupProcedures { get; set; }

        #endregion
    }
}
