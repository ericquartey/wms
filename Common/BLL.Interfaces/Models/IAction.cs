using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.Common.BLL.Interfaces.Models
{
    public interface IAction
    {
        #region Properties

        bool IsAllowed { get; }

        string Reason { get; set; }

        #endregion
    }
}
