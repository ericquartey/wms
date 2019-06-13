using System.Collections.Generic;

namespace Ferretto.WMS.Data.Core.Models
{
    public class MachineServiceInfo : BaseModel<int>
    {
        #region Properties

        public IEnumerable<Bay> Bays { get; set; }

        public string ServiceUrl { get; set; }

        #endregion
    }
}
