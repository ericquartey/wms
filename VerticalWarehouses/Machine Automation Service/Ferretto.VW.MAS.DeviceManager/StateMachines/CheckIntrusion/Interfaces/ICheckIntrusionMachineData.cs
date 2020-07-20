using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;

namespace Ferretto.VW.MAS.DeviceManager.CheckIntrusion.Interfaces
{
    internal interface ICheckIntrusionMachineData : IMachineData
    {
        #region Properties

        IBaysDataProvider BaysDataProvider { get; }

        IMachineResourcesProvider MachineSensorStatus { get; }

        #endregion
    }
}
