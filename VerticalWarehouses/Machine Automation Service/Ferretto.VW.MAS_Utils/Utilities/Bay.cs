using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS_Utils.Utilities
{
    public class Bay
    {
        #region Properties

        public int Id { get; set; }

        public bool IsConnected { get; set; }

        public BlockingConcurrentQueue<Mission> Missions { get; set; }

        public BayStatus Status { get; set; }

        public BayTypes Type { get; set; }

        #endregion
    }
}
