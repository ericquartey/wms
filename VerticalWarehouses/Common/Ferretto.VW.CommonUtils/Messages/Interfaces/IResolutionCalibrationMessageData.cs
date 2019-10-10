namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IResolutionCalibrationMessageData : IMessageData
    {
        #region Properties

        double MeasuredInitialPosition { get; }

        double ReadFinalPosition { get; }

        decimal Resolution { get; set; }

        #endregion
    }
}
