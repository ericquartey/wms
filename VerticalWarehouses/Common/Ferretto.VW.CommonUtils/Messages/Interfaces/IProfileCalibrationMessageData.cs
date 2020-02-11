namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IProfileCalibrationMessageData : IMessageData
    {
        #region Properties

        double? ProfileCalibrateDistance { get; }

        double? ProfileStartDistance { get; }

        #endregion
    }
}
