using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.WMS.Data.Core.Models
{
    public class MachineWithdraw : BaseModel<int>
    {
        #region Properties

        public int? AvailableQuantityItem { get; set; }

        public string Nickname { get; set; }

        #endregion
    }
}
