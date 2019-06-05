using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class CompartmentType : BaseModel<int>, ICompartmentTypeDeletePolicy
    {
        #region Properties

        public int CompartmentsCount { get; set; }

        public int EmptyCompartmentsCount { get; set; }

        public double? Height { get; set; }

        public int ItemCompartmentsCount { get; set; }

        public double? Width { get; set; }

        #endregion
    }
}
