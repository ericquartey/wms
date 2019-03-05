using System.Collections.Generic;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class Area : Model
    {
        #region Properties

        public IEnumerable<Bay> Bays { get; set; }

        #endregion
    }
}
