using System.Collections.Generic;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface IItemPutPolicy : IModel<int>
    {
        #region Properties

        bool HasCompartmentTypes { get; set; }

        #endregion
    }
}
