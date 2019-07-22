using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IExecuteMissionMessageData : IMessageData
    {
        #region Properties

        string BayConnectionId { get; set; }

        Mission Mission { get; set; }

        MissionOperation MissionOperation { get; set; }

        int PendingMissionsCount { get; set; }

        #endregion
    }
}
