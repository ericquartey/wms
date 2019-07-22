namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IBayConnectedMessageData : IMessageData
    {
        #region Properties

        int BayId { get; set; }

        // TODO change type from int to BayType
        int BayType { get; set; }

        int PendingMissionsCount { get; set; }

        #endregion
    }
}
