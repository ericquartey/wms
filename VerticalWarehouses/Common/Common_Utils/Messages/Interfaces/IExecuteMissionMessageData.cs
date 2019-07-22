using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IExecuteMissionMessageData : IMessageData
    {
        #region Properties

        string BayConnectionId { get; set; }

        MissionInfo Mission { get; set; }

        MissionOperationInfo MissionOperation { get; set; }

        int PendingMissionsCount { get; set; }

        #endregion
    }
}
