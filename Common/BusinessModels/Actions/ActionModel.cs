using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.Common.BusinessModels
{
    public class ActionModel : IAction
    {
        #region Properties

        public bool IsAllowed { get; set; }

        public string Reason { get; set; }

        #endregion
    }
}
