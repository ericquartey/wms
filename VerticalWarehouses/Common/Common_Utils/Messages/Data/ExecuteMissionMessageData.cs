using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class ExecuteMissionMessageData : IExecuteMissionMessageData
    {
        #region Constructors

        public ExecuteMissionMessageData()
        {
        }

        public ExecuteMissionMessageData(Mission mission, int missionsQuantity, string connectionId)
        {
            this.Mission = mission;
            this.MissionsQuantity = missionsQuantity;
            this.BayConnectionId = connectionId;
        }

        #endregion

        #region Properties

        public string BayConnectionId { get; set; }

        public Mission Mission { get; set; }

        public int MissionsQuantity { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"BayConnectionId:{this.BayConnectionId} Mission:{this.Mission} MissionsQuantity:{this.MissionsQuantity}";
        }

        #endregion
    }
}
