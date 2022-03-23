using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface ISensorsProvider
    {
        #region Properties

        bool IsDrawerPartiallyOnCradle { get; }

        bool IsMachineSecurityRunning { get; }

        bool IsSensorZeroOnCradle { get; }

        #endregion

        #region Methods

        bool[] GetAll();
        int[] GetOutCurrent();
        bool[] GetOutFault();
        ShutterPosition GetShutterPosition(InverterIndex inverterIndex);

        bool IsLoadingUnitInLocation(LoadingUnitLocation location);

        /// <summary>
        /// CHecks if all security sensors are set to allow running state
        /// </summary>
        /// <returns></returns>
        bool IsMachineSecureForRun(out string errorText, out MachineErrorCode errorCode, out BayNumber bayNumber);

        #endregion
    }
}
