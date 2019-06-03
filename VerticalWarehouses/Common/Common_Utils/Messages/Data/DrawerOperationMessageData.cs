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
    public class DrawerOperationMessageData : IDrawerOperationMessageData
    {
        #region Constructors

        public DrawerOperationMessageData(Mission mission)
        {
            this.Mission = mission;
        }

        #endregion

        #region Properties

        public Mission Mission { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
