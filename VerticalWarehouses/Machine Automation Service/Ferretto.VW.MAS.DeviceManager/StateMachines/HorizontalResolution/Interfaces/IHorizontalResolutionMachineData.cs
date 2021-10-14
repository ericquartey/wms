using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.DeviceManager.HorizontalResolution.Interfaces
{
    internal interface IHorizontalResolutionMachineData : IMachineData
    {
        #region Properties

        IBaysDataProvider BaysDataProvider { get; }

        InverterIndex CurrentInverterIndex { get; }

        int ExecutedSteps { get; set; }

        IMachineResourcesProvider MachineSensorStatus { get; }

        IPositioningMessageData MessageData { get; set; }

        MessageActor Requester { get; }

        #endregion
    }
}
