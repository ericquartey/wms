namespace Ferretto.VW.MAS_InverterDriver
{
    public interface IInverterDriver
    {
        #region Methods

        void Destroy();

        void ExecuteHorizontalHoming();

        void ExecuteHorizontalPosition();

        void ExecuteVerticalHoming();

        void ExecuteVerticalPosition(int target, float weight);

        float GetDrawerWeight();

        bool[] GetSensorsStates();

        #endregion Methods
    }
}
