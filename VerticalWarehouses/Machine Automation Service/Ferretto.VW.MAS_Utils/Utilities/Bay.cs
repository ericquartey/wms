using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS_Utils.Utilities
{
    public class Bay
    {
        #region Constructors

        public Bay()
        {
            this.Missions = new Queue<Mission>();
        }

        #endregion

        #region Properties

        public string ConnectionId { get; set; }

        public int Id { get; set; }

        public string IpAddress { get; set; }

        public bool IsConnected { get; set; }

        public Queue<Mission> Missions { get; set; }

        public BayStatus Status { get; set; }

        public BayTypes Type { get; set; }

        #endregion
    }
}
