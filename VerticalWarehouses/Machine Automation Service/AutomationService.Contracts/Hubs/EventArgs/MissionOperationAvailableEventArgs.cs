using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.MAS.AutomationService.Contracts.Hubs
{
    public class MissionOperationAvailableEventArgs : System.EventArgs
    {
        #region Constructors

        public MissionOperationAvailableEventArgs(MissionOperationInfo missionOperation)
        {
            this.MissionOperation = missionOperation;
        }

        #endregion

        #region Properties

        public MissionOperationInfo MissionOperation { get; }

        #endregion
    }
}
