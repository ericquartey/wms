namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IElevatorWeightCheckProcedureProvider
    {
        #region Methods

        void Start(int loadingUnitId, decimal runToTest, decimal weight);

        void Stop();

        #endregion
    }
}
