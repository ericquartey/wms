namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IResolutionCalibrationMessageData : IMessageData
    {
        #region Properties

        double ReadFinalPosition { get; }

        double MeasuredInitialPosition { get; }

        decimal Resolution { get; set; }

        #endregion
    }
}
