using Ferretto.WMS.Data.Core.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class CompartmentSetForPick : CompartmentSet, IOrderableCompartment
    {
        #region Properties

        public double Availability { get; set; }

        #endregion
    }
}
