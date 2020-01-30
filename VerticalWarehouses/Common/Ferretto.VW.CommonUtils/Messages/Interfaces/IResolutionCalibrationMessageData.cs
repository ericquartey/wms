namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IResolutionCalibrationMessageData : IMessageData
    {
        #region Properties

        double MeasuredInitialPosition { get; }

        double ReadFinalPosition { get; }

        double Resolution { get; set; }

        #endregion
    }
}
