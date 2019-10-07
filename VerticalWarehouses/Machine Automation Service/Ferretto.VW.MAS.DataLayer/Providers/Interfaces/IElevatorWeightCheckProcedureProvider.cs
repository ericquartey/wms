namespace Ferretto.VW.MAS.DataLayer
{
    public interface IElevatorWeightCheckProcedureProvider
    {
        #region Methods

        void Start(int loadingUnitId, double displacement, double weight);

        void Stop();

        #endregion
    }
}
