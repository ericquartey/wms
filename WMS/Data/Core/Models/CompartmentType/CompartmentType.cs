using System;
using Ferretto.Common.Utils;
using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    [Resource(nameof(CompartmentType))]
    public class CompartmentType : BaseModel<int>, ICompartmentTypeDeletePolicy
    {
        #region Properties

        public int CompartmentsCount { get; set; }

        public int EmptyCompartmentsCount { get; set; }

        public double? Height { get; set; }

        public int ItemCompartmentsCount { get; set; }

        public double? Width { get; set; }

        #endregion

        #region Methods

        public bool ApplyCorrection(double increment)
        {
            this.Width = Math.Floor(this.Width.Value / increment) * increment;
            this.Height = Math.Floor(this.Height.Value / increment) * increment;

            return this.Width.Value.CompareTo(0) != 0 && this.Height.Value.CompareTo(0) != 0;
        }

        #endregion
    }
}
