using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS_Utils.Utilities.Interfaces;

namespace Ferretto.VW.MAS_Utils.Utilities
{
    public class BaysManager : IBaysManager
    {
        #region Fields

        private readonly IList<Bay> bays = new List<Bay>();

        #endregion

        #region Properties

        public IEnumerable<Bay> Bays => this.bays;

        #endregion

        #region Methods

        public void SetupBays(IEnumerable<Bay> bays)
        {
            this.bays.Clear();
            foreach (var bay in bays)
            {
                this.bays.Add(bay);
            }
        }

        #endregion
    }
}
