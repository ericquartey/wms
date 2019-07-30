namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface INewMissionOperationAvailable
    {
        #region Properties

        int BayId { get; set; }

        int MissionId { get; set; }

        int MissionOperationId { get; set; }

        int PendingMissionsCount { get; set; }

        #endregion
    }
}
