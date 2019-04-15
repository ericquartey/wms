using System;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public interface IOrderableCompartment
    {
        #region Properties

        double Availability { get; }

        DateTime? FirstStoreDate { get; }

        #endregion
    }
}
