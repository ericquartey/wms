using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;

namespace Ferretto.VW.App.Controls.Interfaces
{
    public interface ISensorsService
    {
        #region Properties

        bool IsLoadingUnitInBay { get; }

        bool IsLoadingUnitInMiddleBottomBay { get; }

        bool IsLoadingUnitOnElevator { get; }

        bool IsZeroChain { get; }

        string LoadingUnitPositionDownInBayCode { get; }

        string LoadingUnitPositionUpInBayCode { get;  }

        Sensors Sensors { get; }

        ShutterSensors ShutterSensors { get; }

        #endregion

        #region Methods

        Task RefreshAsync(bool forceRefresh);

        #endregion
    }
}
