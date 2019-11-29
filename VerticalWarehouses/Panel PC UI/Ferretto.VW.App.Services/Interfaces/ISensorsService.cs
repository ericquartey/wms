using System.Threading.Tasks;

namespace Ferretto.VW.App.Services
{
    public interface ISensorsService
    {
        #region Properties

        bool IsLoadingUnitInBay { get; }

        bool IsLoadingUnitInMiddleBottomBay { get; }

        bool IsLoadingUnitOnElevator { get; }

        bool IsZeroChain { get; }

        Sensors Sensors { get; }

        ShutterSensors ShutterSensors { get; }

        #endregion

        #region Methods

        Task RefreshAsync(bool forceRefresh);

        #endregion
    }
}
