using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IElevatorDataProvider
    {
        #region Methods

        double ComputeBeltDisplacement(double targetPosition);

        int ConvertMillimetersToPulses(double millimeters, Orientation orientation);

        double ConvertPulsesToMillimeters(int pulses, Orientation orientation);

        ElevatorAxis GetHorizontalAxis();

        LoadingUnit GetLoadingUnitOnBoard();

        double GetMaximumLoadOnBoard();

        ElevatorAxis GetVerticalAxis();

        void UpdateVerticalOffset(double newOffset);

        void UpdateVerticalResolution(decimal newResolution);

        #endregion
    }
}
