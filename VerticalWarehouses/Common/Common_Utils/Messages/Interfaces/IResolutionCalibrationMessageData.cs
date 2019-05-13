namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IResolutionCalibrationMessageData : IMessageData
    {
        #region Properties

        decimal ReadFinalPosition { get; }

        decimal ReadInitialPosition { get; }

        decimal Resolution { get; set; }

        #endregion
    }
}
