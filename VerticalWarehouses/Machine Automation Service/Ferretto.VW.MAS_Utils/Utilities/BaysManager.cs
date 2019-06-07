using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS_Utils.Utilities.Interfaces;

namespace Ferretto.VW.MAS_Utils.Utilities
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
