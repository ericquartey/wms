namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IOffsetCalibrationDataLayer
    {
        #region Properties

        decimal FeedRateOC { get; }

        int ReferenceCell { get; }

        decimal StepValue { get; }

        #endregion
    }
}
