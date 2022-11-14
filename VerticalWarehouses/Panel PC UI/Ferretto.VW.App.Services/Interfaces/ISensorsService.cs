using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Services
{
    public interface ISensorsService
    {
        #region Events

        event EventHandler<EventArgs> OnUpdateSensors;

        #endregion

        #region Properties

        bool BayRobotOption { get; }

        bool BayTrolleyOption { get; }

        bool BayZeroChain { get; }

        bool BayZeroChainUp { get; }

        bool BayZeroChainUpIsVisible { get; set; }

        bool BEDExternalBayBottom { get; }

        bool BEDExternalBayTop { get; }

        bool BEDInternalBayBottom { get; }

        bool BEDInternalBayTop { get; }

        bool IsBypass { get; set; }

        bool IsExtraVertical { get; }

        bool IsHorizontalInconsistentBothHigh { get; }

        bool IsHorizontalInconsistentBothLow { get; }

        bool IsLoadingUnitInBay { get; }

        bool IsLoadingUnitInMiddleBottomBay { get; }

        bool IsLoadingUnitOnElevator { get; }

        bool IsZeroChain { get; }

        bool IsZeroVertical { get; }

        Sensors Sensors { get; }

        ShutterSensors ShutterSensors { get; }

        ShutterSensors ShutterSensorsBay1 { get; }

        ShutterSensors ShutterSensorsBay2 { get; }

        ShutterSensors ShutterSensorsBay3 { get; }

        #endregion

        #region Methods

        bool IsLoadingUnitInBayByNumber(MAS.AutomationService.Contracts.BayNumber bayNumber);

        bool IsLoadingUnitInMiddleBottomBayByNumber(MAS.AutomationService.Contracts.BayNumber bayNumber);

        Task RefreshAsync(bool forceRefresh);
        void SetBay(Bay bay);

        #endregion
    }
}
