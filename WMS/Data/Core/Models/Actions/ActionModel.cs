using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public class ActionModel : IAction
    {
        #region Properties

        public bool IsAllowed { get; set; }

        public string Reason { get; set; }

        #endregion
    }
}
