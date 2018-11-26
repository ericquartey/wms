using System;

namespace Ferretto.Common.BusinessModels
{
    public interface IOrderableCompartment
    {
        #region Properties

        int Availability { get; set; }
        DateTime? FirstStoreDate { get; set; }

        #endregion Properties
    }
}
