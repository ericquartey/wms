namespace Ferretto.VW.MAS.MissionsManager.Providers
{
    public interface IWeightAnalysisMissionProvider
    {
        #region Methods

        void Start(double initialPosition, double displacement, double netWeight, System.TimeSpan inPlaceSamplingDuration);

        void Stop();

        #endregion
    }
}
