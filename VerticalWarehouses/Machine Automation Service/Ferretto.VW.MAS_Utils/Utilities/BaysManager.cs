using System.Collections.Generic;
using Ferretto.VW.MAS.Utils.Utilities.Interfaces;

namespace Ferretto.VW.MAS.Utils.Utilities
{
    public class BaysManager : IBaysManager
    {
        #region Constructors

        public BaysManager()
        {
            this.Bays = new List<Bay>();
        }

        #endregion

        #region Properties

        public List<Bay> Bays { get; set; }

        #endregion
    }
}
