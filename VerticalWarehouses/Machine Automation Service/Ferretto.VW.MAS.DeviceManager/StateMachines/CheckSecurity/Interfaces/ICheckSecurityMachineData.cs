using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;

namespace Ferretto.VW.MAS.DeviceManager.CheckSecurity.Interfaces
{
    internal interface ICheckSecurityMachineData : IMachineData
    {
        #region Properties

        IBaysDataProvider BaysDataProvider { get; }

        IMachineResourcesProvider MachineSensorStatus { get; }

        #endregion
    }
}
