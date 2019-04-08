using System.Collections.Generic;

namespace Ferretto.WMS.Data.Core.Models
{
    public class LoadingUnitContentInfo : BaseModel<int>
    {
        #region Properties

        public IEnumerable<CompartmentContentInfo> Compartments { get; set; }

        public double Length { get; set; }

        public double Width { get; set; }

        #endregion
    }
}
