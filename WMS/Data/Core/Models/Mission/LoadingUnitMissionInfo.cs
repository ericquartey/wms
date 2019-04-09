using System.Collections.Generic;

namespace Ferretto.WMS.Data.Core.Models
{
    public class LoadingUnitMissionInfo
    {
        #region Properties

        public IEnumerable<CompartmentMissionInfo> Compartments { get; set; }

        public int Id { get; set; }

        public double Length { get; set; }

        public double Width { get; set; }

        #endregion
    }
}
