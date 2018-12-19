using System;

namespace Ferretto.WMS.Scheduler.Core
{
    public interface IOrderableCompartment
    {
        #region Properties

        int Availability { get; }
        DateTime? FirstStoreDate { get; }

        #endregion Properties
    }
}
