using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICompartmentTypeProvider : IBusinessProvider<CompartmentType, CompartmentType>
    {
        #region Methods

        Task<OperationResult> Add(CompartmentType model, int? itemId = null, int? maxCapacity = null);

        #endregion Methods
    }
}
