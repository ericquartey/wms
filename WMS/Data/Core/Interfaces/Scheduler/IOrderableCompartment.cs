using System;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IOrderableCompartment
    {
        #region Properties

        double Availability { get; }

        DateTime? FifoStartDate { get; }

        double RemainingCapacity { get; }

        #endregion
    }
}
