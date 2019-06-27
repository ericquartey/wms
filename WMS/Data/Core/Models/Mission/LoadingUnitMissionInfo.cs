using System.Collections.Generic;
using Ferretto.Common.Utils;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(Mission))]
    public class LoadingUnitMissionInfo
    {
        #region Properties

        public IEnumerable<CompartmentMissionInfo> Compartments { get; set; }

        public int Id { get; set; }

        [Positive]
        public double Depth { get; set; }

        [Positive]
        public double Width { get; set; }

        #endregion
    }
}
