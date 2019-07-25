namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IMissionCompletedMessageData : IMessageData
    {
        #region Properties

        int BayId { get; set; }

        int MissionId { get; set; }

        #endregion
    }
}
