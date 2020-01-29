using System;
using System.Collections.Generic;
using System.Threading;

namespace Ferretto.VW.MAS.DataModels
{
    public class VertimagConfiguration : IValidable
    {
        #region Properties

        public IEnumerable<LoadingUnit> LoadingUnits { get; set; }

        public Machine Machine { get; set; }

        public SetupProceduresSet SetupProcedures { get; set; }

        #endregion

        #region Methods

        public void Validate()
        {
            this.Machine.Validate();
            this.LoadingUnits.ForEach(l => l.Validate());
        }

        #endregion
    }
}
