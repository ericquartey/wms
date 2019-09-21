namespace Ferretto.VW.MAS.MissionsManager.Providers
{
    public interface IWeightCheckMissionProvider
    {
        #region Methods

        void Start(int loadingUnitId, double runToTest, double weight);

        void Stop();

        #endregion
    }
}
