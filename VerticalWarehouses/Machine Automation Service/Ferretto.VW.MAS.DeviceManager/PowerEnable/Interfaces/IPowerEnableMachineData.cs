using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;

namespace Ferretto.VW.MAS.DeviceManager.PowerEnable.Interfaces
{
    internal interface IPowerEnableMachineData : IMachineData
    {
        #region Properties

        bool Enable { get; }

        IMachineResourcesProvider MachineSensorStatus { get; }

        #endregion
    }
}
