using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface ICanBeExecuted
    {
        #region Properties

        bool CanBeExecuted { get; }

        #endregion
    }
}
