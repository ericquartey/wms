using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class BayConnectedMessageData : IBayConnectedMessageData
    {
        #region Properties

        public int BayType { get; set; }

        public int Id { get; set; }

        public int MissionQuantity { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
