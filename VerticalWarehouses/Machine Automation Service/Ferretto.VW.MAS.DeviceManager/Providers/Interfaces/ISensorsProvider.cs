using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DeviceManager.Providers.Interfaces
{
    public interface ISensorsProvider
    {
        #region Properties

        bool IsMachineRunning { get; }

        #endregion

        #region Methods

        bool[] GetAll();

        ShutterPosition GetShutterPosition(BayNumber bayNumber);

        #endregion
    }
}
