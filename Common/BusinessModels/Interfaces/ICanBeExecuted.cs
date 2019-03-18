using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.Common.BusinessModels.Interfaces
{
    public interface ICanBeExecuted
    {
        #region Properties

        bool CanBeExecuted { get; }

        #endregion
    }
}
