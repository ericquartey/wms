using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS_Utils.Utilities.Interfaces;

namespace Ferretto.VW.MAS_Utils.Utilities
{
    public class BaysManager : IBaysManager
    {
        #region Properties

        public IEnumerable<Bay> Bays { get; } = new List<Bay>();

        #endregion
    }
}
