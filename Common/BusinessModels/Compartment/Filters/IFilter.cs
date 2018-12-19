using System;

namespace Ferretto.Common.BusinessModels
{
    public interface IFilter
    {
        #region Properties

        Func<CompartmentDetails, CompartmentDetails, string> ColorFunc { get; }
        string Description { get; }
        int Id { get; }
        CompartmentDetails Selected { get; set; }

        #endregion Properties
    }
}
