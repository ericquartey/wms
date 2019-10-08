namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IResolutionCalibrationMessageData : IMessageData
    {
        #region Properties

        decimal MeasuredInitialPosition { get; }

        decimal ReadFinalPosition { get; }

        decimal Resolution { get; set; }

        #endregion
    }
}
