using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.DeviceManager.ShutterPositioning.Interfaces
{
    internal interface IShutterPositioningMachineData : IMachineData
    {
        #region Properties

        InverterIndex InverterIndex { get; }

        IMachineResourcesProvider MachineSensorsStatus { get; }

        IShutterPositioningMessageData PositioningMessageData { get; }

        #endregion
    }
}
