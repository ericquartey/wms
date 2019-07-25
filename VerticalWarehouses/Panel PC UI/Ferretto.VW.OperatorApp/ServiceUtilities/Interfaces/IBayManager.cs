using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.App.Operator.ServiceUtilities.Interfaces
{
    public interface IBayManager
    {
        #region Properties

        int BayId { get; set; }

        Mission CurrentMission { get; set; }

        int QueuedMissionsQuantity { get; set; }

        #endregion

        #region Methods

        void CompleteCurrentMission();

        #endregion
    }
}
