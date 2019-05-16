using System.Collections.Generic;

namespace Ferretto.WMS.Data.Core.Models
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1056:Uri properties should not be strings",
        Justification = "Validation of Uri string is performed at business level.")]
    public class MachineServiceInfo : Model<int>
    {
        #region Properties

        public IEnumerable<Bay> Bays { get; set; }

        public string ServiceUrl { get; set; }

        #endregion
    }
}
