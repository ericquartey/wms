using Ferretto.VW.MAS.DataModels;

namespace Ferretto.VW.MAS.DataLayer.Providers.Interfaces
{
    public interface IElevatorDataProvider
    {
        #region Methods

        int ConvertMillimetersToPulses(decimal millimeters, Orientation orientation);

        decimal ConvertPulsesToMillimeters(int pulses, Orientation orientation);

        ElevatorAxis GetHorizontalAxis();

        LoadingUnit GetLoadingUnitOnBoard();

        decimal GetMaximumLoadOnBoard();

        ElevatorAxis GetVerticalAxis();

        void UpdateVerticalOffset(decimal newOffset);

        void UpdateVerticalResolution(decimal newResolution);

        #endregion
    }
}
