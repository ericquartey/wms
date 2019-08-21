namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IBayConnectedMessageData : IMessageData
    {
        #region Properties

        int BayType { get; set; }

        int Id { get; set; }

        int MissionQuantity { get; set; }

        #endregion
    }
}
