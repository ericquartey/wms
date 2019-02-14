using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.Common.BLL.Interfaces
{
    public interface IOperationResult
    {
        #region Properties

        string Description { get; }

        int? EntityId { get; }

        bool Success { get; }

        #endregion
    }
}
