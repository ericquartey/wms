using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface ISensorsProvider
    {
        #region Properties

        bool IsMachineSecurityRunning { get; }

        #endregion

        #region Methods

        bool[] GetAll();

        ShutterPosition GetShutterPosition(BayNumber bayNumber);

        bool IsLoadingUnitInLocation(LoadingUnitLocation location);

        /// <summary>
        /// CHecks if all security sensors are set to allow running state
        /// </summary>
        /// <returns></returns>
        bool IsMachineSecureForRun();

        #endregion
    }
}
