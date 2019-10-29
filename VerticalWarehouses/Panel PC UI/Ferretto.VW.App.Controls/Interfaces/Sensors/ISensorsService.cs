using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;

namespace Ferretto.VW.App.Controls.Interfaces
{
    public interface ISensorsService
    {
        #region Properties

        bool IsLoadingUnitInBay { get; }

        bool IsLoadingUnitOnElevator { get; }

        bool IsZeroChain { get; }

        Sensors Sensors { get; }

        ShutterSensors ShutterSensors { get; }

        #endregion

        #region Methods

        Task RefreshAsync();

        #endregion
    }
}
