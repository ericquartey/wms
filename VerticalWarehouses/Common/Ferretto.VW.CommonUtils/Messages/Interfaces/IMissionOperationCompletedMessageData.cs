namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IMissionOperationCompletedMessageData : IMessageData
    {
        #region Properties

        int MissionId { get; set; }

        int MissionOperationId { get; set; }

        #endregion
    }
}
