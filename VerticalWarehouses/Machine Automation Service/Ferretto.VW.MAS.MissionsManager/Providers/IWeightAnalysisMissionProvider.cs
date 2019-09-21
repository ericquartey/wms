namespace Ferretto.VW.MAS.MissionsManager.Providers
{
    public interface IWeightAnalysisMissionProvider
    {


        #region Methods

        void Start(decimal initialPosition, decimal displacement, decimal netWeight, System.TimeSpan inPlaceSamplingDuration);

        void Stop();

        #endregion
    }
}
