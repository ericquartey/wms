namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IOffsetCalibration
    {
        #region Properties

        decimal FeedRateOC { get; }

        int ReferenceCell { get; }

        decimal StepValue { get; }

        #endregion
    }
}
