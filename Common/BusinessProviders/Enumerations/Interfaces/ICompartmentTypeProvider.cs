using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.BusinessProviders
{
    public interface ICompartmentTypeProvider : IReadAllAsyncProvider<Enumeration>
    {
        #region Methods

        Task<IOperationResult> AddAsync(CompartmentType model, int? itemId = null, int? maxCapacity = null);

        #endregion
    }
}
