using System;
using System.Threading.Tasks;

namespace Ferretto.VW.App.Services
{
    public interface ISensorsService
    {
        #region Properties

        bool IsExtraVertical { get; }

        bool IsLoadingUnitInBay { get; }

        bool IsLoadingUnitInMiddleBottomBay { get; }

        bool IsLoadingUnitOnElevator { get; }

        bool IsZeroChain { get; }

        bool IsZeroVertical { get; }

        Sensors Sensors { get; }

        ShutterSensors ShutterSensors { get; }

        bool IsHorizontalInconsistentBothLow { get; }

        bool IsHorizontalInconsistentBothHigh { get; }

        bool BayZeroChain { get; }

        bool BayTrolleyOption { get; }

        bool BayRobotOption { get; }
        

        event EventHandler<EventArgs> OnUpdateSensors;

        #endregion

        #region Methods

        bool IsLoadingUnitInBayByNumber(MAS.AutomationService.Contracts.BayNumber bayNumber);

        bool IsLoadingUnitInMiddleBottomBayByNumber(MAS.AutomationService.Contracts.BayNumber bayNumber);

        Task RefreshAsync(bool forceRefresh);

        #endregion
    }
}
