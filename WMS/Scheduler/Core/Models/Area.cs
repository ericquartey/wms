using System.Collections.Generic;

namespace Ferretto.WMS.Scheduler.Core
{
    public class Area : BusinessObject
    {
        #region Properties

        public IEnumerable<Bay> Bays { get; set; }

        #endregion Properties
    }
}
