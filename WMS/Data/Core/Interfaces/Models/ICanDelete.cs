using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICanDelete
    {
        #region Properties

        bool CanDelete { get; }

        #endregion
    }
}
