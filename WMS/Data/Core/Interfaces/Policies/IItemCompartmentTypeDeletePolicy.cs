using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface IItemCompartmentTypeDeletePolicy
    {
        #region Properties

        double TotalReservedForPick { get; set; }

        double TotalReservedToPut { get; set; }

        double TotalStock { get; set; }

        #endregion
    }
}
