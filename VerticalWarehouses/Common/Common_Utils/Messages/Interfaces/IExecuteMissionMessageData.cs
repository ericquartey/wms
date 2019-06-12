using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IExecuteMissionMessageData : IMessageData
    {
        #region Properties

        Mission Mission { get; set; }

        int MissionsQuantity { get; set; }

        #endregion
    }
}
