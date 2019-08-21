namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IResolutionCalibrationMessageData : IMessageData
    {
        #region Properties

        decimal ReadFinalPosition { get; }

        decimal MeasuredInitialPosition { get; }

        decimal Resolution { get; set; }

        #endregion
    }
}
