namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IElevatorWeightCheckMessageData : IMessageData
    {
        #region Properties

        double? Weight { get; set; }

        #endregion
    }
}
