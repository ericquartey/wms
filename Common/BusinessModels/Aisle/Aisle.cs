using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.Common.BusinessModels
{
    public class Aisle : BusinessObject
    {
        #region Properties

        public int AreaId { get; set; }

        public string AreaName { get; set; }

        public string Name { get; set; }

        #endregion
    }
}
