using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.OperatorApp.ServiceUtilities.Interfaces
{
    public interface IBayManager
    {
        #region Properties

        Mission CurrentMission { get; set; }

        int QueuedMissionsQuantity { get; set; }

        #endregion

        #region Methods

        void CompleteCurrentMission();

        #endregion
    }
}
